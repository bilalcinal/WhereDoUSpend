using FinanceTracker.Domain;

namespace FinanceTracker.Application.Transactions;

public record TransactionCreateDto(decimal Amount, TransactionType Type, DateTime Date, string? Note, int? AccountId, int? CategoryId, int[]? TagIds);
public record TransactionUpdateDto(decimal Amount, TransactionType Type, DateTime Date, string? Note, int? AccountId, int? CategoryId, int[]? TagIds);
public record TransactionVm(int Id, decimal Amount, TransactionType Type, DateTime Date, string? Note, int? AccountId, int? CategoryId, string? CategoryName, string? AccountName, string[] Tags); 