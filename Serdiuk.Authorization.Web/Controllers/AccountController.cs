using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serdiuk.Authorization.Web.Data;
using Serdiuk.Authorization.Web.Data.IdentityModels;
using Serdiuk.Authorization.Web.Infrastructure;
using Serdiuk.Authorization.Web.Models;
using Serdiuk.Authorization.Web.Models.DTO;
using System.Security.Claims;

namespace Serdiuk.Authorization.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public AccountController(UserManager<IdentityUser> userManager, IConfiguration configuration, ITokenService tokenService, AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _tokenService = tokenService;
            _context = context;
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

            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsRevoked = false,
                Token = refreshToken,
                UserId = userExists.Id,
            });
            await _context.SaveChangesAsync();
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

            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsRevoked = false,
                Token = refresh,
                UserId = user.Id,
            });
            await _context.SaveChangesAsync();

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

            var refreshToken = await _context.RefreshTokens.FirstAsync(x => x.Token == model.RefreshToken);

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
            var user = await _context.Users.FirstAsync(x => x.Id == refreshToken.UserId);

            var newAccessToken = _tokenService.GenerateAccessToken(user, _configuration);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            refreshToken.IsRevoked = true;

            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsRevoked = false,
                Token = newRefreshToken,
                UserId = user.Id,
            });

            return Ok(new AuthResponce
            {
                Refresh = newRefreshToken,
                Result = true,
                Token = newAccessToken,
            });
        }
    }
}