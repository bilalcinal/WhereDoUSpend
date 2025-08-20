using FinanceTracker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinanceTracker.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
	private readonly AppDbContext _db;
	private readonly DbSet<TEntity> _set;

	public Repository(AppDbContext db)
	{
		_db = db;
		_set = db.Set<TEntity>();
	}

	public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
		=> await _set.FindAsync([id], ct);

	public async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
		=> predicate == null ? await _set.ToListAsync(ct) : await _set.Where(predicate).ToListAsync(ct);

	public async Task AddAsync(TEntity entity, CancellationToken ct = default)
	{
		await _set.AddAsync(entity, ct);
	}

	public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
	{
		_set.Update(entity);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(TEntity entity, CancellationToken ct = default)
	{
		_set.Remove(entity);
		return Task.CompletedTask;
	}

	public IQueryable<TEntity> Query() => _set.AsQueryable();
} 