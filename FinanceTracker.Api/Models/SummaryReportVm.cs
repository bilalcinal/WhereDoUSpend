using FinanceTracker.Domain;

namespace FinanceTracker.Api.Models;

public class SummaryReportVm
{
    public string Category { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Total { get; set; }
} 