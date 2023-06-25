using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Serdiuk.Authorization.Web.Infrastructure;

namespace Serdiuk.Authorization.Web.Data
{
    public class DataInitializer
    {
        static string username = "dev@serdiuk.com";
        static string password = "123qwe";
        static string phone = "+380000000000";
        public static async Task Intialize(IServiceProvider provider)
        {
            var context = provider.GetService<AppDbContext>();
            var isExists = (context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();


            if (!isExists) return; // TODO : Throw db exception

            await context.Database.MigrateAsync();

            var userManager = provider.GetService<UserManager<IdentityUser>>();
            var roleManager = provider.GetService<RoleManager<IdentityRole>>();
            var roles = AppData.Roles.ToArray();
            IdentityResult identityResult;

            if (userManager == null || roleManager == null)
            {
                throw new Exception("UserManager or RoleManager not registered"); 
                // TODO : Change to self exception
            }

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            if (await userManager.FindByEmailAsync(username) is not null)
            {
                return;
            }

            var user = new IdentityUser
            {
                UserName = username,
                Email = username,
                EmailConfirmed = true,
                PhoneNumber = phone,
                PhoneNumberConfirmed = true
            };
            identityResult = await userManager.CreateAsync(user, password);
            IdentityResultHandler(identityResult);

            identityResult = await userManager.AddToRolesAsync(user, roles);
            IdentityResultHandler(identityResult);

            await context.SaveChangesAsync();
        }

        private static void IdentityResultHandler(IdentityResult result)
        {
            if (result.Succeeded is false)
            {
                var message = string.Join(", ", result.Errors.Select(x => $"{x.Code}: {x.Description}"));
                throw new Exception(message); // TODO : Change to self exception
            }
        }
    }
}
