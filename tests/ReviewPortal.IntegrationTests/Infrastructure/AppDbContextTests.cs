using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Infrastructure.Data;

namespace ReviewPortal.IntegrationTests.Infrastructure;

public class AppDbContextTests
{
    [Fact]
    public void Model_ShouldIncludeCoreAggregateRoots()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ReviewPortalModelTests;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var context = new AppDbContext(options);

        Assert.NotNull(context.Model.FindEntityType(typeof(Category)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Tool)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Review)));
        Assert.NotNull(context.Model.FindEntityType(typeof(User)));
    }

    [Fact]
    public async Task DateTimeProperties_WhenReadFromSqlServer_AreMarkedAsUtc()
    {
        var databaseName = $"ReviewPortalUtcDateTimeTests_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        var databaseTimestamp = new DateTime(2026, 6, 13, 12, 30, 0, DateTimeKind.Unspecified);

        try
        {
            await using (var setupContext = new AppDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();

                var tool = new Tool
                {
                    Category = new Category
                    {
                        Name = "UTC Test Category",
                        Description = "Used by UTC DateTime integration tests"
                    },
                    Name = "UTC Test Tool",
                    Description = "Used by UTC DateTime integration tests",
                    HourlyRate = 5m,
                    DailyRate = 20m,
                    WeeklyRate = 80m,
                    IsActive = true
                };

                setupContext.Reviews.Add(new Review
                {
                    Tool = tool,
                    ReviewerName = "UTC Tester",
                    ReviewerEmail = "utc.tester@example.com",
                    ReviewText = "This review verifies SQL Server timestamps are read back as UTC values.",
                    EquipmentRating = 5,
                    CustomerServiceRating = 5,
                    TechnicalSupportRating = 5,
                    AfterSalesRating = 5,
                    ValueForMoneyRating = 5,
                    OverallRating = 5m,
                    Status = ReviewStatus.Pending,
                    CreatedDate = databaseTimestamp
                });

                await setupContext.SaveChangesAsync();
            }

            await using var verifyContext = new AppDbContext(options);
            var review = await verifyContext.Reviews.SingleAsync();

            Assert.Equal(DateTimeKind.Utc, review.CreatedDate.Kind);
            Assert.Equal(databaseTimestamp.Ticks, review.CreatedDate.Ticks);
        }
        finally
        {
            await using var cleanupContext = new AppDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }
}
