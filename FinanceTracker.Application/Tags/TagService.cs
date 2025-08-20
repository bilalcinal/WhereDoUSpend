using FinanceTracker.Domain;
using FinanceTracker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Tags;

public class TagService : ITagService
{
    private readonly IRepository<Tag> _repo;
    private readonly IUnitOfWork _uow;

    public TagService(IRepository<Tag> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<(IReadOnlyList<TagVm> Items, int Total)> ListAsync(string userId, int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = _repo.Query().Where(t => t.UserId == userId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => EF.Functions.Like(t.Name, $"%{search}%"));
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TagVm(t.Id, t.Name))
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<TagVm> CreateAsync(string userId, TagCreateDto dto, CancellationToken ct)
    {
        var exists = await _repo.Query().AnyAsync(t => t.UserId == userId && t.Name == dto.Name, ct);
        if (exists) throw new InvalidOperationException("Tag exists");
        var entity = new Tag { UserId = userId, Name = dto.Name };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return new TagVm(entity.Id, entity.Name);
    }
} 