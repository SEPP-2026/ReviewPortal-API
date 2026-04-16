using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Entities;
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
}
