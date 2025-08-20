using FinanceTracker.Domain;
using FinanceTracker.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace FinanceTracker.Api;

public class DevSeederHostedService : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly IHostEnvironment _env;
    public DevSeederHostedService(IServiceProvider provider, IHostEnvironment env)
    {
        _provider = provider;
        _env = env;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment()) return;
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        await DataSeeder.SeedAsync(db, um);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
} 