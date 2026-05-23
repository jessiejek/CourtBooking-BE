using CourtBooking.Domain.Entities;
using CourtBooking.Domain.Entities.Authentication;
using CourtBooking.Domain.Entities.Scoring;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CourtBooking.Infrastructure.Seeding;

public class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Persistence.ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Seed roles
        var roles = new[] { "Admin", "Staff", "User", "Umpire" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user
        if (await userManager.FindByEmailAsync("admin@courtbooking.com") == null)
        {
            var admin = new ApplicationUser
            {
                FullName = "System Admin",
                Email = "admin@courtbooking.com",
                UserName = "admin@courtbooking.com",
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        // Seed ScoringRequiresBooking setting
        var scoringSetting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == "ScoringRequiresBooking");
        if (scoringSetting == null)
        {
            context.AppSettings.Add(new AppSetting
            {
                Id = Guid.NewGuid(),
                Key = "ScoringRequiresBooking",
                Value = "false",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Seed ScoreSport: Pickleball
        var pickleball = await context.ScoreSports.FirstOrDefaultAsync(s => s.Code == "PICKLEBALL");
        if (pickleball == null)
        {
            pickleball = new ScoreSport
            {
                Id = Guid.NewGuid(),
                Code = "PICKLEBALL",
                Name = "Pickleball",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.ScoreSports.Add(pickleball);
        }

        // Seed ScoreRuleSet
        var ruleSet = await context.ScoreRuleSets.FirstOrDefaultAsync(r => r.Code == "PICKLEBALL_SIDE_OUT_11");
        if (ruleSet == null)
        {
            context.ScoreRuleSets.Add(new ScoreRuleSet
            {
                Id = Guid.NewGuid(),
                SportId = pickleball.Id,
                Code = "PICKLEBALL_SIDE_OUT_11",
                Name = "Pickleball Side-Out to 11",
                ScoringMode = "SideOut",
                TargetScore = 11,
                WinBy = 2,
                IsDefault = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}
