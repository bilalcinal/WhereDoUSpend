using FinanceTracker.Domain;
using FinanceTracker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Transactions;

public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _repo;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Account> _accounts;
    private readonly IRepository<Tag> _tags;
    private readonly IUnitOfWork _uow;

    public TransactionService(
        IRepository<Transaction> repo,
        IRepository<Category> categories,
        IRepository<Account> accounts,
        IRepository<Tag> tags,
        IUnitOfWork uow)
    {
        _repo = repo;
        _categories = categories;
        _accounts = accounts;
        _tags = tags;
        _uow = uow;
    }

    public async Task<(IReadOnlyList<TransactionVm> Items, int Total)> ListAsync(string userId, DateTime? from, DateTime? to, int? accountId, int? categoryId, string? sort, int page, int pageSize, CancellationToken ct)
    {
        IQueryable<Transaction> query = _repo.Query()
            .Where(t => t.UserId == userId);

        query = query.Include(t => t.Category)
            .Include(t => t.Account)
            .Include(t => t.TransactionTags)
                .ThenInclude(tt => tt.Tag)
            .AsQueryable();

        if (from.HasValue) query = query.Where(t => t.Date >= from.Value);
        if (to.HasValue) query = query.Where(t => t.Date <= to.Value);
        if (accountId.HasValue) query = query.Where(t => t.AccountId == accountId);
        if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId);

        query = sort?.ToLowerInvariant() switch
        {
            "date:asc" => query.OrderBy(t => t.Date),
            _ => query.OrderByDescending(t => t.Date)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionVm(
                t.Id,
                t.Amount,
                t.Type,
                t.Date,
                t.Note,
                t.AccountId,
                t.CategoryId,
                t.Category != null ? t.Category.Name : null,
                t.Account != null ? t.Account.Name : null,
                t.TransactionTags.Select(x => x.Tag.Name).ToArray()))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<TransactionVm> CreateAsync(string userId, TransactionCreateDto dto, CancellationToken ct)
    {
        var entity = new Transaction
        {
            UserId = userId,
            Amount = dto.Amount,
            Type = dto.Type,
            Date = dto.Date,
            Note = dto.Note,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId
        };

        if (dto.TagIds != null && dto.TagIds.Length > 0)
        {
            entity.TransactionTags = dto.TagIds.Select(id => new TransactionTag { TagId = id, Transaction = entity }).ToList();
        }

        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return new TransactionVm(entity.Id, entity.Amount, entity.Type, entity.Date, entity.Note, entity.AccountId, entity.CategoryId, null, null, Array.Empty<string>());
    }

    public async Task<TransactionVm> UpdateAsync(string userId, int id, TransactionUpdateDto dto, CancellationToken ct)
    {
        var entity = await _repo.Query().Include(t => t.TransactionTags).FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
        if (entity == null) throw new KeyNotFoundException();
        entity.Amount = dto.Amount;
        entity.Type = dto.Type;
        entity.Date = dto.Date;
        entity.Note = dto.Note;
        entity.AccountId = dto.AccountId;
        entity.CategoryId = dto.CategoryId;

        // replace tags
        entity.TransactionTags.Clear();
        if (dto.TagIds != null)
        {
            foreach (var tagId in dto.TagIds)
            {
                entity.TransactionTags.Add(new TransactionTag { TransactionId = entity.Id, TagId = tagId });
            }
        }

        await _uow.SaveChangesAsync(ct);
        return new TransactionVm(entity.Id, entity.Amount, entity.Type, entity.Date, entity.Note, entity.AccountId, entity.CategoryId, null, null, Array.Empty<string>());
    }

    public async Task DeleteAsync(string userId, int id, CancellationToken ct)
    {
        var entity = await _repo.Query().FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
        if (entity == null) return;
        entity.DeletedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }
} 