using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serdiuk.Authorization.Web.Data;
using Serdiuk.Authorization.Web.Infrastructure.Interfaces;

namespace Serdiuk.Authorization.Web.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityUser> GetUserById(string id)
        {
            var user = await _context.Users.FirstAsync(x => x.Id == id);
            return user;
        }
    }
}
