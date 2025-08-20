namespace FinanceTracker.Application.Auth;

public record RegisterDto(string Email, string Password);
public record LoginDto(string Email, string Password);
public record RefreshDto(string RefreshToken);

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt); 