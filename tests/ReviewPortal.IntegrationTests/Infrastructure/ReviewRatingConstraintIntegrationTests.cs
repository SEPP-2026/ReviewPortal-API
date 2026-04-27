using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Infrastructure.Data;

namespace ReviewPortal.IntegrationTests.Infrastructure;

public class ReviewRatingConstraintIntegrationTests
{
    [Fact]
    public async Task SaveChangesAsync_WithOutOfRangeReviewRating_FailsAtDatabaseLevel()
    {
        var databaseName = $"ReviewPortalRatingConstraintTests_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        await using var context = new AppDbContext(options);

        try
        {
            await context.Database.EnsureCreatedAsync();

            var category = new Category
            {
                Name = "Constraint Test Category",
                Description = "Used by rating constraint integration tests"
            };

            var tool = new Tool
            {
                Category = category,
                Name = "Constraint Test Tool",
                Description = "Used by rating constraint integration tests",
                HourlyRate = 5m,
                DailyRate = 20m,
                WeeklyRate = 80m,
                IsActive = true
            };

            context.Tools.Add(tool);
            await context.SaveChangesAsync();

            context.Reviews.Add(new Review
            {
                ToolId = tool.Id,
                ReviewerName = "Constraint Tester",
                ReviewerEmail = "constraint.tester@example.com",
                ReviewText = "This review intentionally has an invalid rating to exercise the database check constraint.",
                EquipmentRating = 0,
                CustomerServiceRating = 5,
                TechnicalSupportRating = 5,
                AfterSalesRating = 5,
                ValueForMoneyRating = 5,
                OverallRating = 4m,
                Status = ReviewStatus.Pending
            });

            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            Assert.Contains("CK_Reviews_EquipmentRating_Range", exception.InnerException?.Message ?? exception.Message);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}
