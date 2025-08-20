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
        return await _transactions.Query()
            .Where(t => t.UserId == userId && t.Date >= start && t.Date < end)
            .GroupBy(t => new { t.CategoryId, t.Type })
            .Select(g => new SummaryItem(
                g.Key.CategoryId.HasValue ? _categories.Query().Where(c => c.Id == g.Key.CategoryId.Value).Select(c => c.Name).FirstOrDefault() ?? "Uncategorized" : "Uncategorized",
                (int)g.Key.Type,
                g.Sum(x => x.Type == TransactionType.Income ? x.Amount : -x.Amount)))
            .ToListAsync(ct);
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
        return await _transactions.Query()
            .Where(t => t.UserId == userId && t.Date >= from && t.Date <= to && t.AccountId != null)
            .GroupBy(t => t.AccountId)
            .Select(g => new AccountTotalItem(
                _accounts.Query().Where(a => a.Id == g.Key).Select(a => a.Name).FirstOrDefault() ?? "Unknown",
                g.Sum(x => x.Type == TransactionType.Income ? x.Amount : -x.Amount)))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BudgetVsActualItem>> BudgetVsActualAsync(string userId, int year, int month, CancellationToken ct)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var budgets = _budgets.Query().Where(b => b.UserId == userId && b.Year == year && b.Month == month);
        var actuals = _transactions.Query().Where(t => t.UserId == userId && t.Date >= start && t.Date < end);

        var result = await budgets
            .Select(b => new BudgetVsActualItem(
                _categories.Query().Where(c => c.Id == b.CategoryId).Select(c => c.Name).FirstOrDefault() ?? "Unknown",
                b.Amount,
                actuals.Where(t => t.CategoryId == b.CategoryId && t.Type == TransactionType.Expense).Sum(t => t.Amount)))
            .ToListAsync(ct);
        return result;
    }
} 