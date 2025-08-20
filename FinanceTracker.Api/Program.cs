using FinanceTracker.Api.Models;
using FinanceTracker.Domain;
using FinanceTracker.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Seed data for development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(dbContext);
}

// Helper function to get user ID from header
string GetUserId(HttpContext context) => 
    context.Request.Headers["x-user-id"].FirstOrDefault() ?? "demo";

// Health check endpoint
app.MapGet("/health", () => "ok");

// Categories endpoints
app.MapGet("/api/categories", async (AppDbContext db, HttpContext context) =>
{
    var userId = GetUserId(context);
    var categories = await db.Categories
        .Where(c => c.UserId == userId)
        .Select(c => new CategoryVm { Id = c.Id, Name = c.Name })
        .ToListAsync();
    
    return Results.Ok(categories);
});

app.MapPost("/api/categories", async (AppDbContext db, HttpContext context, UpsertCategoryDto dto) =>
{
    var userId = GetUserId(context);
    
    // Check if category with same name already exists for this user
    var existing = await db.Categories
        .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == dto.Name);
    
    if (existing != null)
        return Results.BadRequest("Category with this name already exists");
    
    var category = new Category
    {
        Name = dto.Name,
        UserId = userId
    };
    
    db.Categories.Add(category);
    await db.SaveChangesAsync();
    
    return Results.Created($"/api/categories/{category.Id}", 
        new CategoryVm { Id = category.Id, Name = category.Name });
});

app.MapDelete("/api/categories/{id}", async (AppDbContext db, HttpContext context, int id) =>
{
    var userId = GetUserId(context);
    var category = await db.Categories
        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    
    if (category == null)
        return Results.NotFound();
    
    // Check if category has transactions
    var hasTransactions = await db.Transactions
        .AnyAsync(t => t.CategoryId == id);
    
    if (hasTransactions)
        return Results.BadRequest("Cannot delete category with existing transactions");
    
    db.Categories.Remove(category);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
});

// Transactions endpoints
app.MapGet("/api/transactions", async (AppDbContext db, HttpContext context, 
    DateTime? from, DateTime? to) =>
{
    var userId = GetUserId(context);
    var query = db.Transactions
        .Where(t => t.UserId == userId)
        .Include(t => t.Category)
        .AsQueryable();
    
    if (from.HasValue)
        query = query.Where(t => t.Date >= from.Value);
    
    if (to.HasValue)
        query = query.Where(t => t.Date <= to.Value);
    
    var transactions = await query
        .OrderByDescending(t => t.Date)
        .Select(t => new TransactionVm
        {
            Id = t.Id,
            Amount = t.Amount,
            Type = t.Type,
            Date = t.Date,
            Note = t.Note,
            CategoryId = t.CategoryId,
            CategoryName = t.Category != null ? t.Category.Name : null
        })
        .ToListAsync();
    
    return Results.Ok(transactions);
});

app.MapPost("/api/transactions", async (AppDbContext db, HttpContext context, UpsertTransactionDto dto) =>
{
    var userId = GetUserId(context);
    
    // Validate category exists if provided
    if (dto.CategoryId.HasValue)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId.Value && c.UserId == userId);
        
        if (category == null)
            return Results.BadRequest("Invalid category");
    }
    
    var transaction = new Transaction
    {
        UserId = userId,
        Amount = dto.Amount,
        Type = dto.Type,
        Date = dto.Date,
        Note = dto.Note,
        CategoryId = dto.CategoryId
    };
    
    db.Transactions.Add(transaction);
    await db.SaveChangesAsync();
    
    return Results.Created($"/api/transactions/{transaction.Id}", transaction);
});

app.MapDelete("/api/transactions/{id}", async (AppDbContext db, HttpContext context, int id) =>
{
    var userId = GetUserId(context);
    var transaction = await db.Transactions
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    
    if (transaction == null)
        return Results.NotFound();
    
    db.Transactions.Remove(transaction);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
});

// Reports endpoints
app.MapGet("/api/reports/summary", async (AppDbContext db, HttpContext context, 
    int year, int month) =>
{
    var userId = GetUserId(context);
    
    var startDate = new DateTime(year, month, 1);
    var endDate = startDate.AddMonths(1).AddDays(-1);
    
    var summary = await db.Transactions
        .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
        .GroupBy(t => new { t.CategoryId, t.Type })
        .Select(g => new SummaryReportVm
        {
            Category = g.Key.CategoryId.HasValue 
                ? db.Categories.Where(c => c.Id == g.Key.CategoryId.Value).Select(c => c.Name).FirstOrDefault() ?? "Unknown"
                : "Uncategorized",
            Type = g.Key.Type,
            Total = g.Sum(t => t.Amount)
        })
        .OrderBy(s => s.Category)
        .ThenBy(s => s.Type)
        .ToListAsync();
    
    return Results.Ok(summary);
});

app.Run();
