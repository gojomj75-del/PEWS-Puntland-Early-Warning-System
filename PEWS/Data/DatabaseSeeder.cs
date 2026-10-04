using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PEWS.Models;

namespace PEWS.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        await SeedSuperAdminAsync(userManager, roleManager);
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        const string superAdminEmail = "superadmin@pews.gov.so";

        if (await userManager.FindByEmailAsync(superAdminEmail) != null)
            return;

        var superAdmin = new ApplicationUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            IsActive = true,
            Region = "Nugaal",
            City = "Garowe",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(superAdmin, "Admin@123456");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            await userManager.AddToRoleAsync(superAdmin, "Admin");
        }
    }
}
