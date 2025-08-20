namespace FinanceTracker.Application.Auth;

public record OperationResult(bool Succeeded, string[] Errors)
{
    public static OperationResult Success() => new(true, Array.Empty<string>());
    public static OperationResult Failure(params string[] errors) => new(false, errors);
} 