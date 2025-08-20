using FluentValidation;
using FinanceTracker.Application.Categories;
using FinanceTracker.Application.Accounts;
using FinanceTracker.Application.Transactions;
using FinanceTracker.Application.Budgets;
using FinanceTracker.Application.Recurring;
using FinanceTracker.Application.Tags;

namespace FinanceTracker.Application.Validation;

public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
    }
}

public class AccountCreateDtoValidator : AbstractValidator<AccountCreateDto>
{
    public AccountCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0);
    }
}

public class TransactionCreateDtoValidator : AbstractValidator<TransactionCreateDto>
{
    public TransactionCreateDtoValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Date).NotEmpty();
    }
}

public class BudgetCreateDtoValidator : AbstractValidator<BudgetCreateDto>
{
    public BudgetCreateDtoValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
    }
}

public class RecurringCreateDtoValidator : AbstractValidator<RecurringCreateDto>
{
    public RecurringCreateDtoValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.NextRunAt).NotEmpty();
    }
}

public class TagCreateDtoValidator : AbstractValidator<TagCreateDto>
{
    public TagCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
    }
} 