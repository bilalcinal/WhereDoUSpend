namespace FinanceTracker.Application.Recurring;

public interface IRecurringService
{
    Task<IReadOnlyList<RecurringVm>> ListAsync(string userId, CancellationToken ct);
    Task<RecurringVm> CreateAsync(string userId, RecurringCreateDto dto, CancellationToken ct);
    Task<int> RunDueAsync(string userId, DateTime? now, CancellationToken ct);
    Task DeleteAsync(string userId, int id, CancellationToken ct);
} 