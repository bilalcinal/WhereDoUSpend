namespace FinanceTracker.Application.Budgets;

public record BudgetCreateDto(int CategoryId, int Year, int Month, decimal Amount);
public record BudgetUpdateDto(decimal Amount);
public record BudgetVm(int Id, int CategoryId, int Year, int Month, decimal Amount); 