namespace FinanceTracker.Domain;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<TransactionTag> TransactionTags { get; set; } = new List<TransactionTag>();
} 