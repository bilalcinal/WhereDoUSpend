namespace FinanceTracker.Application.Accounts;

public interface IAccountService
{
    Task<(IReadOnlyList<AccountVm> Items, int Total)> ListAsync(string userId, int page, int pageSize, CancellationToken ct);
    Task<AccountVm> CreateAsync(string userId, AccountCreateDto dto, CancellationToken ct);
    Task ArchiveAsync(string userId, int id, CancellationToken ct);
    Task DeleteAsync(string userId, int id, CancellationToken ct);
} 