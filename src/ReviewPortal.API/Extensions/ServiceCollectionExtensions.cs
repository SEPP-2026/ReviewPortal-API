using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.Extensions.Options;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Application.Services;
using ReviewPortal.Application.Validators.Users;
using ReviewPortal.Domain.Interfaces;
using ReviewPortal.Infrastructure.Authentication;
using ReviewPortal.Infrastructure.Data;
using ReviewPortal.Infrastructure.Repositories;
using ReviewPortal.Infrastructure.Services;

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

        if (ContainsPlaceholder(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'DefaultConnection' still contains placeholder values. Replace them in user secrets, environment variables, or ignored appsettings.Local.json.");
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

        // File storage
        services.Configure<ImageStorageOptions>(configuration.GetSection(ImageStorageOptions.SectionName));
        services.AddSingleton(CreateBlobContainerClient);
        services.AddScoped<IBlobImageStorage, AzureBlobImageStorage>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application services will be registered here as they are implemented
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IToolService, ToolService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }

    private static bool ContainsPlaceholder(string value)
    {
        return value.Contains('<') || value.Contains('>');
    }

    private static BlobContainerClient CreateBlobContainerClient(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<ImageStorageOptions>>().Value;
        var containerName = string.IsNullOrWhiteSpace(options.ContainerName)
            ? "tool-images"
            : options.ContainerName.Trim();

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return new BlobContainerClient(options.ConnectionString, containerName);
        }

        if (!Uri.TryCreate(options.ServiceUri, UriKind.Absolute, out var serviceUri))
        {
            throw new InvalidOperationException("Azure Blob Storage is not configured. Set ImageStorage:ConnectionString for local development, or ImageStorage:ServiceUri for managed identity access.");
        }

        var serviceClient = new BlobServiceClient(serviceUri, new DefaultAzureCredential());
        return serviceClient.GetBlobContainerClient(containerName);
    }
}
