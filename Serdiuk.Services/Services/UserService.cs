using Microsoft.EntityFrameworkCore;
using Serdiuk.Authorization.Domain.IdentityModels;
using Serdiuk.Persistance.Data;
using Serdiuk.Services.Interfaces;

namespace Serdiuk.Services.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            var user = await _context.Users.FirstAsync(x => x.Id == id);
            return user;
        }
    }
}
