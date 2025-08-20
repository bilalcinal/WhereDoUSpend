using FinanceTracker.Domain;

namespace FinanceTracker.Application.Categories;

public interface ICategoryService
{
    Task<(IReadOnlyList<CategoryVm> Items, int Total)> ListAsync(string userId, int page, int pageSize, string? search, CancellationToken ct);
    Task<CategoryVm> CreateAsync(string userId, CategoryCreateDto dto, CancellationToken ct);
    Task<CategoryVm> UpdateAsync(string userId, int id, CategoryUpdateDto dto, CancellationToken ct);
    Task DeleteAsync(string userId, int id, CancellationToken ct);
} 