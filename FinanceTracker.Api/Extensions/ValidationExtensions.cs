using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.Api.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddAppValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<FinanceTracker.Application.Validation.CategoryCreateDtoValidator>();
        return services;
    }
} 