using FinanceTracker.Domain;

namespace FinanceTracker.Application.Recurring;

public record RecurringCreateDto(int? AccountId, int CategoryId, decimal Amount, TransactionType Type, Cadence Cadence, DateTime NextRunAt, string? Note);
public record RecurringVm(int Id, int? AccountId, int CategoryId, decimal Amount, TransactionType Type, Cadence Cadence, DateTime NextRunAt, string? Note); 