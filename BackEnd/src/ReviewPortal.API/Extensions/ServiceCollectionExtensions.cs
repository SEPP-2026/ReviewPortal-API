using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Interfaces;
using ReviewPortal.Infrastructure.Data;
using ReviewPortal.Infrastructure.Repositories;

namespace ReviewPortal.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application services will be registered here as they are implemented
        // e.g. services.AddScoped<IToolService, ToolService>();
        // e.g. services.AddScoped<ICategoryService, CategoryService>();
        // e.g. services.AddScoped<IReviewService, ReviewService>();
        // e.g. services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
