using FinanceTracker.Domain;
using FinanceTracker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Budgets;

public class BudgetService : IBudgetService
{
    private readonly IRepository<Budget> _repo;
    private readonly IUnitOfWork _uow;

    public BudgetService(IRepository<Budget> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<IReadOnlyList<BudgetVm>> ListAsync(string userId, int year, int month, CancellationToken ct)
    {
        return await _repo.Query().Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .Select(b => new BudgetVm(b.Id, b.CategoryId, b.Year, b.Month, b.Amount))
            .ToListAsync(ct);
    }

    public async Task<BudgetVm> CreateAsync(string userId, BudgetCreateDto dto, CancellationToken ct)
    {
        var entity = new Budget
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Year = dto.Year,
            Month = dto.Month,
            Amount = dto.Amount
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return new BudgetVm(entity.Id, entity.CategoryId, entity.Year, entity.Month, entity.Amount);
    }

    public async Task<BudgetVm> UpdateAsync(string userId, int id, BudgetUpdateDto dto, CancellationToken ct)
    {
        var entity = await _repo.Query().FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, ct);
        if (entity == null) throw new KeyNotFoundException();
        entity.Amount = dto.Amount;
        await _uow.SaveChangesAsync(ct);
        return new BudgetVm(entity.Id, entity.CategoryId, entity.Year, entity.Month, entity.Amount);
    }

    public async Task DeleteAsync(string userId, int id, CancellationToken ct)
    {
        var entity = await _repo.Query().FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, ct);
        if (entity == null) return;
        entity.DeletedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
    }
} 