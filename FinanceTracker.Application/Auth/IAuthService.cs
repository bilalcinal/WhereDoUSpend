namespace FinanceTracker.Application.Auth;

public interface IAuthService
{
    Task<OperationResult> RegisterAsync(string email, string password, CancellationToken ct);
    Task<TokenResponse> LoginAsync(string email, string password, CancellationToken ct);
    Task<TokenResponse> RefreshAsync(string refreshToken, CancellationToken ct);
    Task LogoutAsync(string userId, string refreshToken, CancellationToken ct);
} 