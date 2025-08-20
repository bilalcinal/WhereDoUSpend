namespace FinanceTracker.Domain;

public class RecurringTransaction : BaseEntity
{
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public Cadence Cadence { get; set; }
    public DateTime NextRunAt { get; set; }
    public string? Note { get; set; }
} 