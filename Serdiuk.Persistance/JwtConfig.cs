using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Serdiuk.Persistance
{
    public class JwtConfig
    {
        private readonly IConfiguration _config;
        byte[] key;

        public JwtConfig(IConfiguration config)
        {
            _config = config;
            key = Encoding.ASCII.GetBytes(_config.GetSection("JwtConfig:SecretKey").Value);
        }

        public TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,//dev only
                ValidateAudience = false, // Dev only
                RequireExpirationTime = false,// dev only   
                ValidateLifetime = true,

            };
        }
    }
}
