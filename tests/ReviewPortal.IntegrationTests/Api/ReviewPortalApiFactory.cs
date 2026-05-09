using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Infrastructure.Authentication;
using ReviewPortal.Infrastructure.Data;
using ReviewPortal.Infrastructure.Services;

namespace ReviewPortal.IntegrationTests.Api;

[CollectionDefinition("ReviewPortal API")]
public class ReviewPortalApiCollection : ICollectionFixture<ReviewPortalApiFactory>
{
    public const string CollectionName = "ReviewPortal API";
}

public sealed class ReviewPortalApiFactory : WebApplicationFactory<Program>
{
    public const string CustomerEmail = "api.customer@example.com";
    public const string CustomerPassword = "Customer123";
    public const string AdminEmail = "api.admin@example.com";
    public const string AdminPassword = "Admin123";
    public const string ModeratorEmail = "api.moderator@example.com";
    public const string ModeratorPassword = "Moderator123";

    private readonly string _databaseName = $"ReviewPortalApiTests_{Guid.NewGuid():N}";
    private bool _databaseDeleted;

    public int AccessCategoryId { get; private set; }
    public int BreakingCategoryId { get; private set; }
    public int TowerScaffoldToolId { get; private set; }
    public int RotaryHammerToolId { get; private set; }
    public int RetiredLadderToolId { get; private set; }

    public string ConnectionString => $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

    public TestBlobImageStorage BlobStorage => Services.GetRequiredService<TestBlobImageStorage>();

    public ReviewPortalApiFactory()
    {
        SetStartupConfiguration();
    }

    public HttpClient CreateHttpsClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
        await ClearSeededDataAsync(context);
        await SeedTestDataAsync(context);
        BlobStorage.Clear();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(CreateConfiguration());
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBlobImageStorage>();
            services.AddSingleton<TestBlobImageStorage>();
            services.AddSingleton<IBlobImageStorage>(provider => provider.GetRequiredService<TestBlobImageStorage>());
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_databaseDeleted)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            using var context = new AppDbContext(options);
            context.Database.EnsureDeleted();
            _databaseDeleted = true;
        }

        base.Dispose(disposing);
    }

    private void SetStartupConfiguration()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", ConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", "IntegrationTestsUseASecretWithAtLeastThirtyTwoChars");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "ReviewPortal.IntegrationTests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "ReviewPortal.IntegrationTests");
        Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", "60");
        Environment.SetEnvironmentVariable("ImageStorage__ContainerName", "tool-images");
        Environment.SetEnvironmentVariable("ImageStorage__PublicBaseUrl", TestBlobImageStorage.PublicBaseUrl);
        Environment.SetEnvironmentVariable("ImageStorage__BlobNamePrefix", "tools");
    }

    private Dictionary<string, string?> CreateConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["Jwt:Secret"] = "IntegrationTestsUseASecretWithAtLeastThirtyTwoChars",
            ["Jwt:Issuer"] = "ReviewPortal.IntegrationTests",
            ["Jwt:Audience"] = "ReviewPortal.IntegrationTests",
            ["Jwt:ExpiryMinutes"] = "60",
            ["ImageStorage:ContainerName"] = "tool-images",
            ["ImageStorage:PublicBaseUrl"] = TestBlobImageStorage.PublicBaseUrl,
            ["ImageStorage:BlobNamePrefix"] = "tools"
        };
    }

    private static async Task ClearSeededDataAsync(AppDbContext context)
    {
        context.CompanyResponses.RemoveRange(await context.CompanyResponses.ToListAsync());
        context.ReviewComments.RemoveRange(await context.ReviewComments.ToListAsync());
        context.Reviews.RemoveRange(await context.Reviews.ToListAsync());
        context.ToolImages.RemoveRange(await context.ToolImages.ToListAsync());
        context.Tools.RemoveRange(await context.Tools.ToListAsync());
        context.Categories.RemoveRange(await context.Categories.ToListAsync());
        context.Users.RemoveRange(await context.Users.ToListAsync());

        await context.SaveChangesAsync();
    }

    private async Task SeedTestDataAsync(AppDbContext context)
    {
        var hasher = new IdentityPasswordHasher();
        var customer = CreateUser("API Customer", CustomerEmail, CustomerPassword, UserRole.Customer, hasher);
        var admin = CreateUser("API Admin", AdminEmail, AdminPassword, UserRole.Admin, hasher);
        var moderator = CreateUser("API Moderator", ModeratorEmail, ModeratorPassword, UserRole.Moderator, hasher);

        var access = new Category
        {
            Name = "Access & Lifting",
            Description = "Access towers, platforms, and lifting equipment.",
            ImageUrl = "/images/categories/access.jpg"
        };
        var breaking = new Category
        {
            Name = "Breaking & Drilling",
            Description = "Drills, breakers, and masonry equipment.",
            ImageUrl = "/images/categories/breaking.jpg"
        };

        var tower = new Tool
        {
            Category = access,
            Name = "Tower Scaffold",
            Description = "Mobile scaffold tower for safe access at height.",
            HourlyRate = 10m,
            DailyRate = 45m,
            WeeklyRate = 180m,
            SpecialNotes = "Guard rails included.",
            DepositRequired = true,
            DepositAmount = 75m,
            IsActive = true,
            ReviewCount = 2,
            OverallRating = 4.4m,
            CreatedDate = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc),
            UpdatedDate = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc)
        };
        tower.Images.Add(new ToolImage
        {
            Tool = tower,
            ImageUrl = "/uploads/tools/tower-scaffold.jpg",
            DisplayOrder = 1,
            UploadedDate = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc)
        });
        tower.Reviews.Add(CreateApprovedReview(tower, "Alex Site", "alex.site@example.com", 5, 4, 5, 4, 5, new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc)));
        tower.Reviews.Add(CreateApprovedReview(tower, "Morgan Build", "morgan.build@example.com", 4, 4, 5, 4, 4, new DateTime(2026, 4, 3, 10, 0, 0, DateTimeKind.Utc)));

        var rotaryHammer = new Tool
        {
            Category = breaking,
            Name = "Rotary Hammer",
            Description = "Heavy-duty rotary hammer drill for concrete and masonry.",
            HourlyRate = 8.5m,
            DailyRate = 45m,
            WeeklyRate = 180m,
            SpecialNotes = "Includes SDS bits.",
            DepositRequired = false,
            IsActive = true,
            CreatedDate = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedDate = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc)
        };
        rotaryHammer.Images.Add(new ToolImage
        {
            Tool = rotaryHammer,
            ImageUrl = "/uploads/tools/rotary-hammer.jpg",
            DisplayOrder = 1,
            UploadedDate = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc)
        });

        var retiredLadder = new Tool
        {
            Category = access,
            Name = "Retired Ladder",
            Description = "Inactive ladder listing kept for admin coverage.",
            HourlyRate = 5m,
            DailyRate = 25m,
            WeeklyRate = 90m,
            DepositRequired = false,
            IsActive = false,
            CreatedDate = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc),
            UpdatedDate = new DateTime(2026, 4, 1, 11, 0, 0, DateTimeKind.Utc)
        };

        context.Users.AddRange(customer, admin, moderator);
        context.Categories.AddRange(access, breaking);
        context.Tools.AddRange(tower, rotaryHammer, retiredLadder);
        await context.SaveChangesAsync();

        AccessCategoryId = access.Id;
        BreakingCategoryId = breaking.Id;
        TowerScaffoldToolId = tower.Id;
        RotaryHammerToolId = rotaryHammer.Id;
        RetiredLadderToolId = retiredLadder.Id;
    }

    private static User CreateUser(
        string name,
        string email,
        string password,
        UserRole role,
        IdentityPasswordHasher hasher)
    {
        return new User
        {
            Name = name,
            Email = email,
            PasswordHash = hasher.Hash(password),
            Role = role,
            CreatedDate = new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc)
        };
    }

    private static Review CreateApprovedReview(
        Tool tool,
        string reviewerName,
        string reviewerEmail,
        int equipmentRating,
        int customerServiceRating,
        int technicalSupportRating,
        int afterSalesRating,
        int valueForMoneyRating,
        DateTime createdDate)
    {
        var review = new Review
        {
            Tool = tool,
            ReviewerName = reviewerName,
            ReviewerEmail = reviewerEmail,
            ReviewText = $"{reviewerName} left detailed feedback for the tower scaffold hire experience.",
            EquipmentRating = equipmentRating,
            CustomerServiceRating = customerServiceRating,
            TechnicalSupportRating = technicalSupportRating,
            AfterSalesRating = afterSalesRating,
            ValueForMoneyRating = valueForMoneyRating,
            Status = ReviewStatus.Approved,
            CreatedDate = createdDate
        };
        review.CalculateOverallRating();
        return review;
    }
}

public sealed class TestBlobImageStorage : IBlobImageStorage
{
    public const string PublicBaseUrl = "https://reviewportaltest.blob.core.windows.net/tool-images";

    private readonly Dictionary<string, StoredBlob> _blobs = new(StringComparer.OrdinalIgnoreCase);

    public int BlobCount => _blobs.Count;

    public async Task<string> UploadAsync(
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        content.Position = 0;
        await content.CopyToAsync(memoryStream, cancellationToken);
        _blobs[blobName] = new StoredBlob(memoryStream.ToArray(), contentType);

        return $"{PublicBaseUrl}/{blobName}";
    }

    public Task DeleteAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        _blobs.Remove(GetBlobName(imageUrl));
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _blobs.Clear();
    }

    public bool ContainsUrl(string imageUrl)
    {
        return _blobs.ContainsKey(GetBlobName(imageUrl));
    }

    public string GetContentType(string imageUrl)
    {
        return _blobs[GetBlobName(imageUrl)].ContentType;
    }

    private static string GetBlobName(string imageUrl)
    {
        return imageUrl.StartsWith($"{PublicBaseUrl}/", StringComparison.OrdinalIgnoreCase)
            ? imageUrl[(PublicBaseUrl.Length + 1)..]
            : imageUrl;
    }

    private sealed record StoredBlob(byte[] Bytes, string ContentType);
}
