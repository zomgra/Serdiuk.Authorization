using Microsoft.AspNetCore.Identity;

namespace Serdiuk.Authorization.Web.Infrastructure.Interfaces
{
    public interface IUserService
    {
        Task<IdentityUser> GetUserById(string id);
    }
}
