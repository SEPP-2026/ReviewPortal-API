using Microsoft.EntityFrameworkCore;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Application.Services;
using ReviewPortal.Domain.Interfaces;
using ReviewPortal.Infrastructure.Data;
using ReviewPortal.Infrastructure.Repositories;
using ReviewPortal.Infrastructure.Authentication;

namespace ReviewPortal.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured. Use user secrets or the ConnectionStrings__DefaultConnection environment variable.");
        }

        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IToolRepository, ToolRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Authentication Providers
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application services will be registered here as they are implemented
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IToolService, ToolService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
