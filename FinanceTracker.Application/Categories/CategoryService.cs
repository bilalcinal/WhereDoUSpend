using FinanceTracker.Application.Categories;
using FinanceTracker.Domain;
using FinanceTracker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Categories;

public class CategoryService : ICategoryService
{
	private readonly IRepository<Category> _repo;
	private readonly IUnitOfWork _uow;

	public CategoryService(IRepository<Category> repo, IUnitOfWork uow)
	{
		_repo = repo;
		_uow = uow;
	}

	public async Task<(IReadOnlyList<CategoryVm> Items, int Total)> ListAsync(string userId, int page, int pageSize, string? search, CancellationToken ct)
	{
		var query = _repo.Query().Where(c => c.UserId == userId);
		if (!string.IsNullOrWhiteSpace(search))
			query = query.Where(c => EF.Functions.Like(c.Name, $"%{search}%"));

		var total = await query.CountAsync(ct);
		var items = await query
			.OrderBy(c => c.Name)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(c => new CategoryVm(c.Id, c.Name))
			.ToListAsync(ct);
		return (items, total);
	}

	public async Task<CategoryVm> CreateAsync(string userId, CategoryCreateDto dto, CancellationToken ct)
	{
		var exists = await _repo.Query().AnyAsync(c => c.UserId == userId && c.Name == dto.Name, ct);
		if (exists) throw new InvalidOperationException("Category already exists");
		var entity = new Category { UserId = userId, Name = dto.Name };
		await _repo.AddAsync(entity, ct);
		await _uow.SaveChangesAsync(ct);
		return new CategoryVm(entity.Id, entity.Name);
	}

	public async Task<CategoryVm> UpdateAsync(string userId, int id, CategoryUpdateDto dto, CancellationToken ct)
	{
		var entity = await _repo.Query().FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
		if (entity == null) throw new KeyNotFoundException();
		entity.Name = dto.Name;
		await _uow.SaveChangesAsync(ct);
		return new CategoryVm(entity.Id, entity.Name);
	}

	public async Task DeleteAsync(string userId, int id, CancellationToken ct)
	{
		var entity = await _repo.Query().FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
		if (entity == null) return;
		entity.DeletedAt = DateTime.UtcNow;
		await _uow.SaveChangesAsync(ct);
	}
} 