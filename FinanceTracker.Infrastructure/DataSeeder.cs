using FinanceTracker.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<AppUser> userManager)
    {
        // Ensure database created
        await context.Database.EnsureCreatedAsync();

        // Seed demo user
        var demoEmail = "demo@local";
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == demoEmail);
        if (user == null)
        {
            user = new AppUser { UserName = demoEmail, Email = demoEmail, EmailConfirmed = true };
            await userManager.CreateAsync(user, "Pass123$");
        }

        if (await context.Categories.AnyAsync())
            return;

        var demoUserId = user.Id;
        var categories = new[]
        {
            new Category { Name = "Food", UserId = demoUserId },
            new Category { Name = "Travel", UserId = demoUserId },
            new Category { Name = "Bills", UserId = demoUserId },
            new Category { Name = "Salary", UserId = demoUserId }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var now = DateTime.Now;
        var transactions = new[]
        {
            new Transaction { UserId = demoUserId, Amount = 2500m, Type = TransactionType.Income, Date = now.AddDays(-30), Note = "Monthly salary", CategoryId = categories.First(c => c.Name == "Salary").Id },
            new Transaction { UserId = demoUserId, Amount = 120.50m, Type = TransactionType.Expense, Date = now.AddDays(-2), Note = "Grocery", CategoryId = categories.First(c => c.Name == "Food").Id },
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
    }
} 