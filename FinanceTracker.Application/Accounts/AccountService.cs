using FinanceTracker.Domain;
using FinanceTracker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Accounts;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _repo;
    private readonly IUnitOfWork _uow;

    public AccountService(IRepository<Account> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<(IReadOnlyList<AccountVm> Items, int Total)> ListAsync(string userId, int page, int pageSize, CancellationToken ct)
    {
        var query = _repo.Query().Where(a => a.UserId == userId && !a.IsArchived);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AccountVm(a.Id, a.Name, a.Type, a.Currency, a.OpeningBalance, a.IsArchived))
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<AccountVm> CreateAsync(string userId, AccountCreateDto dto, CancellationToken ct)
    {
        var exists = await _repo.Query().AnyAsync(a => a.UserId == userId && a.Name == dto.Name, ct);
        if (exists) throw new InvalidOperationException("Account exists");
        var entity = new Account
        {
            UserId = userId,
            Name = dto.Name,
            Type = dto.Type,
            Currency = dto.Currency,
            OpeningBalance = dto.OpeningBalance
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return new AccountVm(entity.Id, entity.Name, entity.Type, entity.Currency, entity.OpeningBalance, entity.IsArchived);
    }

    public async Task ArchiveAsync(string userId, int id, CancellationToken ct)
    {
        var entity = await _repo.Query().FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);
        if (entity == null) return;
        entity.IsArchived = true;
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string userId, int id, CancellationToken ct)
    {
        var entity = await _repo.Query().FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);
        if (entity == null) return;
        entity.DeletedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }
} 