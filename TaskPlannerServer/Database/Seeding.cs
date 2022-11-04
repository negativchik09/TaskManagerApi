using Microsoft.AspNetCore.Identity;
using TaskPlannerServer.Auth;

namespace TaskPlannerServer.Database;

public static class Seeding
{
    public static async void InitializeRoles(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

        if (roleManager == null)
        {
            return;
        }

        string[] roles = { UserRoles.Manager, UserRoles.User };
        
        foreach (string role in roles)
        {
            if (!roleManager.Roles.Any(r => r.Name == role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        
        roleManager.Dispose();
    }

    public static async void InitializeUsers(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>();
        
        if (userManager == null)
        {
            return;
        }
        
        IdentityUser user;
        
        var universalPassword = "aboba";

        var managerLogins = new string[]
        {
            "first_manager",
            "second_manager"
        };

        var userLogins = new string[]
        {
            "Oleh",
            "Ihor",
            "Robert",
            "Nick"
        };

        foreach (var manager in managerLogins)
        {
            if (userManager.Users.Any(u => u.UserName == manager)) continue;

            user = new IdentityUser(manager);
            var result = await userManager.CreateAsync(user, universalPassword);

            await userManager.AddToRoleAsync(user, UserRoles.Manager);
        }
        
        foreach (var userName in userLogins)
        {
            if (userManager.Users.Any(u => u.UserName == userName)) continue;

            user = new IdentityUser(userName);
            var result = await userManager.CreateAsync(user, universalPassword);

            await userManager.AddToRoleAsync(user, UserRoles.User);
        }
        
        userManager.Dispose();
    }
}