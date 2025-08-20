namespace FinanceTracker.Application.Categories;

public record CategoryCreateDto(string Name);
public record CategoryUpdateDto(string Name);
public record CategoryVm(int Id, string Name); 