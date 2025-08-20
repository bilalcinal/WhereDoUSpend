namespace FinanceTracker.Domain;

public class Transaction : BaseEntity
{
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public int? CategoryId { get; set; }
    
    public Category? Category { get; set; }
    public ICollection<TransactionTag> TransactionTags { get; set; } = new List<TransactionTag>();
} 