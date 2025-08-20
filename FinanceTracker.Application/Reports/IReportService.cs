namespace FinanceTracker.Application.Reports;

public record SummaryItem(string Category, int Type, decimal Total);
public record CashflowItem(DateTime Period, decimal Net);
public record AccountTotalItem(string Account, decimal Total);
public record BudgetVsActualItem(string Category, decimal Budget, decimal Actual);

public interface IReportService
{
    Task<IReadOnlyList<SummaryItem>> SummaryAsync(string userId, int year, int month, CancellationToken ct);
    Task<IReadOnlyList<CashflowItem>> CashflowAsync(string userId, DateTime from, DateTime to, string groupBy, CancellationToken ct);
    Task<IReadOnlyList<AccountTotalItem>> ByAccountAsync(string userId, DateTime from, DateTime to, CancellationToken ct);
    Task<IReadOnlyList<BudgetVsActualItem>> BudgetVsActualAsync(string userId, int year, int month, CancellationToken ct);
} 