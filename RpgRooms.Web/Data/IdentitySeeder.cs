using Microsoft.AspNetCore.Identity;
using RpgRooms.Infrastructure.Data;

namespace RpgRooms.Web.Data;

public class IdentitySeeder(UserManager<ApplicationUser> userManager)
{
    public async Task SeedAsync()
    {
        if (await userManager.FindByNameAsync("admin") is null)
        {
            var user = new ApplicationUser { UserName = "admin", Email = "admin@example.com", DisplayName = "Admin" };
            await userManager.CreateAsync(user, "admin"); // apenas dev
        }
    }
}
