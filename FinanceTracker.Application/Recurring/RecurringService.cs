using FinanceTracker.Domain;
using FinanceTracker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Recurring;

public class RecurringService : IRecurringService
{
    private readonly IRepository<RecurringTransaction> _recurrings;
    private readonly IRepository<Transaction> _transactions;
    private readonly IUnitOfWork _uow;

    public RecurringService(IRepository<RecurringTransaction> recurrings, IRepository<Transaction> transactions, IUnitOfWork uow)
    {
        _recurrings = recurrings;
        _transactions = transactions;
        _uow = uow;
    }

    public async Task<IReadOnlyList<RecurringVm>> ListAsync(string userId, CancellationToken ct)
    {
        return await _recurrings.Query().Where(r => r.UserId == userId)
            .Select(r => new RecurringVm(r.Id, r.AccountId, r.CategoryId, r.Amount, r.Type, r.Cadence, r.NextRunAt, r.Note))
            .ToListAsync(ct);
    }

    public async Task<RecurringVm> CreateAsync(string userId, RecurringCreateDto dto, CancellationToken ct)
    {
        var entity = new RecurringTransaction
        {
            UserId = userId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Type = dto.Type,
            Cadence = dto.Cadence,
            NextRunAt = dto.NextRunAt,
            Note = dto.Note
        };
        await _recurrings.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return new RecurringVm(entity.Id, entity.AccountId, entity.CategoryId, entity.Amount, entity.Type, entity.Cadence, entity.NextRunAt, entity.Note);
    }

    public async Task<int> RunDueAsync(string userId, DateTime? now, CancellationToken ct)
    {
        var utcNow = now ?? DateTime.UtcNow;
        var due = await _recurrings.Query().Where(r => r.UserId == userId && r.NextRunAt <= utcNow).ToListAsync(ct);
        foreach (var r in due)
        {
            var t = new Transaction
            {
                UserId = userId,
                AccountId = r.AccountId,
                CategoryId = r.CategoryId,
                Amount = r.Amount,
                Type = r.Type,
                Date = utcNow,
                Note = r.Note
            };
            await _transactions.AddAsync(t, ct);
            r.NextRunAt = r.Cadence switch
            {
                Cadence.Daily => r.NextRunAt.AddDays(1),
                Cadence.Weekly => r.NextRunAt.AddDays(7),
                Cadence.Monthly => r.NextRunAt.AddMonths(1),
                _ => r.NextRunAt
            };
        }
        var count = due.Count;
        await _uow.SaveChangesAsync(ct);
        return count;
    }

    public async Task DeleteAsync(string userId, int id, CancellationToken ct)
    {
        var entity = await _recurrings.Query().FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct);
        if (entity == null) return;
        entity.DeletedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }
} 