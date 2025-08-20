namespace FinanceTracker.Domain;

public class TransactionLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Data { get; set; } = string.Empty; // JSON snapshot
} 