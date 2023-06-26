using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serdiuk.Authorization.Web.Infrastructure.Interfaces;
using Serdiuk.Authorization.Web.Models;
using Serdiuk.Authorization.Web.Models.DTO;

namespace Serdiuk.Authorization.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public AccountController(UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ITokenService tokenService, IUserService userService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _userService = userService;
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

            var jwtToken =_tokenService.GenerateAccessToken(userExists, _configuration);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _tokenService.AddNewRefreshToken(refreshToken, userExists.Id);
            return Ok(new AuthResponce { Result = true, Token = jwtToken, Refresh = refreshToken });
        
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
            var token = _tokenService.GenerateAccessToken(user,_configuration); //Token for front-end (React, Angular etc)
            var refresh = _tokenService.GenerateRefreshToken();
            //await _signInManager.SignInAsync(user, false); For ASP.Net MVC architecture

            await _tokenService.AddNewRefreshToken(refresh, user.Id);

            return Ok(new AuthResponce
            {
                Result = true,
                Token = token,
                Refresh = refresh
            });
        }
        [HttpPost]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto model)
        {
            if (model is null)
                return BadRequest(new AuthResponce
                {
                    Errors = new() { "Invalid request" },
                    Result = false
                });

            var refreshToken = await _tokenService.GetRefreshTokenByTokenAsync(model.RefreshToken); //_context.RefreshTokens.FirstAsync(x => x.Token == model.RefreshToken);

            if (refreshToken == null || refreshToken.IsRevoked)
                return BadRequest(new AuthResponce
                {
                    Errors = new() { "Invalid refresh token" },
                    Result = false
                });

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest(new AuthResponce
                {
                    Errors = new() { "Refresh token expires." },
                    Result = false
                });
            }
            var user = await _userService.GetUserById(refreshToken.UserId);
             
            var newAccessToken = _tokenService.GenerateAccessToken(user, _configuration);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            _tokenService.SetRevokedRefreshToken(refreshToken);

            await _tokenService.AddNewRefreshToken(refreshToken.Token, user.Id);

            return Ok(new AuthResponce
            {
                Refresh = newRefreshToken,
                Result = true,
                Token = newAccessToken,
            });
        }
    }
}