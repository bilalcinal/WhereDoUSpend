namespace FinanceTracker.Domain;

public class Transaction
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public int? CategoryId { get; set; }
    
    // Navigation property
    public Category? Category { get; set; }
} 