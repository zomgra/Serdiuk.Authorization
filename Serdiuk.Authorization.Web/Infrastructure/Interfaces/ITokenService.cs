using Microsoft.AspNetCore.Identity;
using Serdiuk.Authorization.Web.Data.IdentityModels;

namespace Serdiuk.Authorization.Web.Infrastructure.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(IdentityUser user, IConfiguration config);
        string GenerateRefreshToken();
        Task<RefreshToken> GetRefreshTokenByTokenAsync(string token);
        Task AddNewRefreshToken(string token, string userId);
        void SetRevokedRefreshToken(RefreshToken refreshToken);
    }
}
