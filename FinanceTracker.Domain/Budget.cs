namespace FinanceTracker.Domain;

public class Budget : BaseEntity
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
} 