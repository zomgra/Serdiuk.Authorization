using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serdiuk.Authorization.Web.Infrastructure;
using Serdiuk.Authorization.Web.Models;
using Serdiuk.Authorization.Web.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Serdiuk.Authorization.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists == null)
                return BadRequest(new AuthResponce
                {
                    Errors = new() { "Invalid payload" },
                    Result = false,
                });
            var isCorrect = await _userManager.CheckPasswordAsync(userExists, model.Password);
            if (!isCorrect)
                return BadRequest(new AuthResponce
                {
                    Errors = new() { "Invalid credentials" }, Result = false,
                });

            var jwtToken = GenerateJwtToken(userExists);
            return Ok(new AuthResponce { Result = true, Token = jwtToken });
        
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if(userExists != null)
            {
                return BadRequest(new AuthResponce
                {
                    Errors = new() { "User already exists" },
                    Result = false,
                });
            }
            var user = new IdentityUser()
            {
                Email = model.Email,
                UserName = model.Name,
            };
            var createdResult = await _userManager.CreateAsync(user, model.Password);
            if (!createdResult.Succeeded)
            {
                return BadRequest(new AuthResponce {
                    Result = false,
                Errors = new() { "Server error, try again" },
                });    
            }
            var token = GenerateJwtToken(user); //Token for front-end (React, Angular etc)

            //await _signInManager.SignInAsync(user, false); For ASP.Net MVC architecture

            return Ok(new AuthResponce
            {
                Result = true,
                Token = token,
            });
        }
        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtConfig:SecretKey").Value);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
                }),
                Expires = DateTime.Now.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)    
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
