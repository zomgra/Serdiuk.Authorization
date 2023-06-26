using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serdiuk.Authorization.Web.Data;
using Serdiuk.Authorization.Web.Data.IdentityModels;
using Serdiuk.Authorization.Web.Infrastructure.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Serdiuk.Authorization.Web.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public TokenService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task AddNewRefreshToken(string token, string userId)
        {
            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsRevoked = false,
                Token = token,
                UserId = userId,
            });
            await _context.SaveChangesAsync();
        }

        public string GenerateAccessToken(IdentityUser user, IConfiguration config)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(config.GetSection("JwtConfig:SecretKey").Value);
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

        public string GenerateRefreshToken()
        {
            var numbers = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(numbers);
                return Convert.ToBase64String(numbers);
            }
        }

        public async Task<RefreshToken> GetRefreshTokenByTokenAsync(string token)
        {
            return await _context.RefreshTokens.FirstAsync(x => x.Token == token);
        }

        public void SetRevokedRefreshToken(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
        }

        //public ClaimsPrincipal GetPrincipalsFromExpiredToken(string token)
        //{
        //    var parameters = new JwtConfig(_config).GetTokenValidationParameters();
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    SecurityToken securityToken;
        //    var principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
        //    var jwtSecurityToken = securityToken as JwtSecurityToken;

        //    if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        throw new SecurityTokenException("Invalid token");
        //    }
        //    return principal;
        //}
    }
}
