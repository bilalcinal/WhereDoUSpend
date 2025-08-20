namespace FinanceTracker.Domain;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
} 