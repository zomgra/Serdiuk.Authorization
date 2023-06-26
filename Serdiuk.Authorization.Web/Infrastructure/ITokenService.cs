using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Serdiuk.Authorization.Web.Infrastructure
{
    public interface ITokenService
    {
        string GenerateAccessToken(IdentityUser user, IConfiguration config);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalsFromExpiredToken(string token);
    }
}
