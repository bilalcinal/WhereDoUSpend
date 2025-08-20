namespace FinanceTracker.Application.Transactions;

public interface ITransactionService
{
    Task<(IReadOnlyList<TransactionVm> Items, int Total)> ListAsync(string userId, DateTime? from, DateTime? to, int? accountId, int? categoryId, string? sort, int page, int pageSize, CancellationToken ct);
    Task<TransactionVm> CreateAsync(string userId, TransactionCreateDto dto, CancellationToken ct);
    Task<TransactionVm> UpdateAsync(string userId, int id, TransactionUpdateDto dto, CancellationToken ct);
    Task DeleteAsync(string userId, int id, CancellationToken ct);
} 