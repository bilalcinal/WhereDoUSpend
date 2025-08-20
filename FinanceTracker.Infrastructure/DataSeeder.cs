using FinanceTracker.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if no categories exist
        if (await context.Categories.AnyAsync())
            return;

        var demoUserId = "demo";
        
        // Create default categories
        var categories = new[]
        {
            new Category { Name = "Food", UserId = demoUserId },
            new Category { Name = "Travel", UserId = demoUserId },
            new Category { Name = "Bills", UserId = demoUserId },
            new Category { Name = "Salary", UserId = demoUserId },
            new Category { Name = "Entertainment", UserId = demoUserId },
            new Category { Name = "Shopping", UserId = demoUserId }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Create some sample transactions
        var now = DateTime.Now;
        var transactions = new[]
        {
            new Transaction
            {
                UserId = demoUserId,
                Amount = 2500.00m,
                Type = TransactionType.Income,
                Date = now.AddDays(-30),
                Note = "Monthly salary",
                CategoryId = categories.First(c => c.Name == "Salary").Id
            },
            new Transaction
            {
                UserId = demoUserId,
                Amount = 120.50m,
                Type = TransactionType.Expense,
                Date = now.AddDays(-2),
                Note = "Grocery shopping",
                CategoryId = categories.First(c => c.Name == "Food").Id
            },
            new Transaction
            {
                UserId = demoUserId,
                Amount = 85.00m,
                Type = TransactionType.Expense,
                Date = now.AddDays(-5),
                Note = "Gas station",
                CategoryId = categories.First(c => c.Name == "Travel").Id
            },
            new Transaction
            {
                UserId = demoUserId,
                Amount = 150.00m,
                Type = TransactionType.Expense,
                Date = now.AddDays(-10),
                Note = "Electricity bill",
                CategoryId = categories.First(c => c.Name == "Bills").Id
            }
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
    }
} 