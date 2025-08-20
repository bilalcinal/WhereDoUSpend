using FinanceTracker.Domain;

namespace FinanceTracker.Api.Models;

public class UpsertTransactionDto
{
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public int? CategoryId { get; set; }
} 