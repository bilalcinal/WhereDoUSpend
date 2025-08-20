using Microsoft.AspNetCore.Http;

namespace FinanceTracker.Api.Models;

public class ImportTransactionsRequest
{
    public IFormFile File { get; set; } = default!;
} 