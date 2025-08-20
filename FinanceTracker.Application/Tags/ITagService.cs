namespace FinanceTracker.Application.Tags;

public interface ITagService
{
    Task<(IReadOnlyList<TagVm> Items, int Total)> ListAsync(string userId, int page, int pageSize, string? search, CancellationToken ct);
    Task<TagVm> CreateAsync(string userId, TagCreateDto dto, CancellationToken ct);
} 