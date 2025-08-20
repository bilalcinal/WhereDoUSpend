namespace FinanceTracker.Application.Budgets;

public interface IBudgetService
{
    Task<IReadOnlyList<BudgetVm>> ListAsync(string userId, int year, int month, CancellationToken ct);
    Task<BudgetVm> CreateAsync(string userId, BudgetCreateDto dto, CancellationToken ct);
    Task<BudgetVm> UpdateAsync(string userId, int id, BudgetUpdateDto dto, CancellationToken ct);
    Task DeleteAsync(string userId, int id, CancellationToken ct);
} 