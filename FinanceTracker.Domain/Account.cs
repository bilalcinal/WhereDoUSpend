namespace FinanceTracker.Domain;

public class Account : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal OpeningBalance { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
} 