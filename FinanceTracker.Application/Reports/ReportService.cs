using FinanceTracker.Domain;
using FinanceTracker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Reports;

public class ReportService : IReportService
{
    private readonly IRepository<Transaction> _transactions;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Account> _accounts;
    private readonly IRepository<Budget> _budgets;

    public ReportService(IRepository<Transaction> transactions, IRepository<Category> categories, IRepository<Account> accounts, IRepository<Budget> budgets)
    {
        _transactions = transactions;
        _categories = categories;
        _accounts = accounts;
        _budgets = budgets;
    }

    public async Task<IReadOnlyList<SummaryItem>> SummaryAsync(string userId, int year, int month, CancellationToken ct)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        var grouped = await _transactions.Query()
            .Where(t => t.UserId == userId && t.Date >= start && t.Date < end)
            .GroupBy(t => new { t.CategoryId, t.Type })
            .Select(g => new { g.Key.CategoryId, g.Key.Type, Total = g.Sum(x => x.Type == TransactionType.Income ? x.Amount : -x.Amount) })
            .ToListAsync(ct);

        var categoryIds = grouped.Where(x => x.CategoryId.HasValue).Select(x => x.CategoryId!.Value).Distinct().ToList();
        var categories = await _categories.Query()
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);
        var categoryNameById = categories.ToDictionary(c => c.Id, c => c.Name);

        return grouped
            .Select(g => new SummaryItem(
                g.CategoryId.HasValue && categoryNameById.TryGetValue(g.CategoryId.Value, out var name) ? name : "Uncategorized",
                (int)g.Type,
                g.Total))
            .ToList();
    }

    public async Task<IReadOnlyList<CashflowItem>> CashflowAsync(string userId, DateTime from, DateTime to, string groupBy, CancellationToken ct)
    {
        var query = _transactions.Query().Where(t => t.UserId == userId && t.Date >= from && t.Date <= to);
        if (groupBy == "month")
        {
            return await query
                .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .Select(g => new CashflowItem(g.Key, g.Sum(x => x.Type == TransactionType.Income ? x.Amount : -x.Amount)))
                .OrderBy(x => x.Period)
                .ToListAsync(ct);
        }
        else
        {
            return await query
                .GroupBy(t => t.Date.Date)
                .Select(g => new CashflowItem(g.Key, g.Sum(x => x.Type == TransactionType.Income ? x.Amount : -x.Amount)))
                .OrderBy(x => x.Period)
                .ToListAsync(ct);
        }
    }

    public async Task<IReadOnlyList<AccountTotalItem>> ByAccountAsync(string userId, DateTime from, DateTime to, CancellationToken ct)
    {
        var grouped = await _transactions.Query()
            .Where(t => t.UserId == userId && t.Date >= from && t.Date <= to && t.AccountId != null)
            .GroupBy(t => t.AccountId)
            .Select(g => new { AccountId = g.Key!.Value, Total = g.Sum(x => x.Type == TransactionType.Income ? x.Amount : -x.Amount) })
            .ToListAsync(ct);

        var accountIds = grouped.Select(g => g.AccountId).Distinct().ToList();
        var accounts = await _accounts.Query()
            .Where(a => accountIds.Contains(a.Id))
            .Select(a => new { a.Id, a.Name })
            .ToListAsync(ct);
        var accountNameById = accounts.ToDictionary(a => a.Id, a => a.Name);

        return grouped
            .Select(g => new AccountTotalItem(accountNameById.TryGetValue(g.AccountId, out var name) ? name : "Unknown", g.Total))
            .ToList();
    }

    public async Task<IReadOnlyList<BudgetVsActualItem>> BudgetVsActualAsync(string userId, int year, int month, CancellationToken ct)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var budgets = _budgets.Query().Where(b => b.UserId == userId && b.Year == year && b.Month == month);
        var actuals = _transactions.Query().Where(t => t.UserId == userId && t.Date >= start && t.Date < end);

        var projected = await budgets
            .Select(b => new { b.CategoryId, b.Amount })
            .ToListAsync(ct);

        var categoryIds = projected.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categories.Query()
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);
        var categoryNameById = categories.ToDictionary(c => c.Id, c => c.Name);

        var actualMap = await actuals
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(ct);
        var actualByCategory = actualMap.Where(x => x.CategoryId.HasValue).ToDictionary(x => x.CategoryId!.Value, x => x.Total);

        return projected
            .Select(p => new BudgetVsActualItem(
                categoryNameById.TryGetValue(p.CategoryId, out var name) ? name : "Unknown",
                p.Amount,
                actualByCategory.TryGetValue(p.CategoryId, out var act) ? act : 0m))
            .ToList();
    }
} 