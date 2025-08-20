using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceTracker.Application.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using FinanceTracker.Domain;

namespace FinanceTracker.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext db, IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _config = config;
    }

    public async Task<OperationResult> RegisterAsync(string email, string password, CancellationToken ct)
    {
        var user = new AppUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            // Ensure roles
            if (!await _db.Roles.AnyAsync(r => r.Name == "User", ct))
            {
                await _db.Roles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityRole("User"), ct);
                await _db.SaveChangesAsync(ct);
            }
            await _userManager.AddToRoleAsync(user, "User");
        }
        return result.Succeeded ? OperationResult.Success() : OperationResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<TokenResponse> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user == null) throw new UnauthorizedAccessException();
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) throw new UnauthorizedAccessException();
        return await IssueTokensAsync(user, ct);
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, ct);
        if (rt == null || rt.IsRevoked) throw new UnauthorizedAccessException();
        var user = await _userManager.FindByIdAsync(rt.UserId);
        if (user == null) throw new UnauthorizedAccessException();
        rt.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await IssueTokensAsync(user, ct);
    }

    public async Task LogoutAsync(string userId, string refreshToken, CancellationToken ct)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == refreshToken, ct);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<TokenResponse> IssueTokensAsync(AppUser user, CancellationToken ct)
    {
        var jwtSection = _config.GetSection("Jwt");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SigningKey"]!));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["AccessTokenMinutes"]!));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(jwtSection["RefreshTokenDays"]!))
        };
        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync(ct);

        return new TokenResponse(accessToken, refresh.Token, expires);
    }
} 