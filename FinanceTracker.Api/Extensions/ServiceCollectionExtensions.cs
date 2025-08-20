using FinanceTracker.Domain.Abstractions;
using FinanceTracker.Infrastructure;
using FinanceTracker.Infrastructure.Repositories;

namespace FinanceTracker.Api.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPersistence(this IServiceCollection services)
	{
		services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
		services.AddScoped<IUnitOfWork, UnitOfWork>();
		return services;
	}
} 