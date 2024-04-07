using System.Security.Claims;
using Core.Entities;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data.Seeding;

public static class Seed
{
    public static async Task SeedData(AppDbContext dbContext,
        UserManager<AppUser> userManager,
        IConfiguration configuration
    )
    {
        await dbContext.Database.MigrateAsync();
        
        var usersCount = userManager.Users.Count();
        
        if (usersCount == 0)
        {
            var user = new AppUser()
            {
                UserName = configuration["AdminInfo:UserName"]
            };

            var createUserResult = await userManager.CreateAsync(user, configuration["AdminInfo:Password"]);
            
            if (!createUserResult.Succeeded)
            {
                throw new Exception("Error creating user during seeding");
            }

            var allClaims = Permissions
                .GetAllPermissions()
                .Select(x => new Claim("Permission", x));
            
            var addClaimsResult = await userManager.AddClaimsAsync(user, allClaims);

            if (!addClaimsResult.Succeeded)
            {
                throw new Exception("Error adding claims to user");
            }
        }
        
        await dbContext.SaveChangesAsync();
    }
}