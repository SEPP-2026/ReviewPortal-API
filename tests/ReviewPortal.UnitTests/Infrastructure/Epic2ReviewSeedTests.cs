using System.Text.RegularExpressions;

namespace ReviewPortal.UnitTests.Infrastructure;

public class Epic2ReviewSeedTests
{
    [Fact]
    public void SeedEpic2ReviewDataMigration_SeedsApprovedAndPendingReviews()
    {
        var migration = File.ReadAllText(GetRepositoryFile("src", "ReviewPortal.Infrastructure", "Migrations", "20260422223500_SeedEpic2ReviewData.cs"));

        Assert.Contains("MERGE dbo.Reviews", migration);
        Assert.Contains("SET IDENTITY_INSERT dbo.Reviews ON", migration);
        Assert.Equal(7, Regex.Matches(migration, "N'Approved'", RegexOptions.CultureInvariant).Count);
        Assert.Equal(2, Regex.Matches(migration, "N'Pending'", RegexOptions.CultureInvariant).Count);
        Assert.Contains("(4001, 2001", migration);
        Assert.Contains("(4007, 2007", migration);
        Assert.Contains("(4008, 2011", migration);
        Assert.Contains("(4009, 2020", migration);
    }

    [Fact]
    public void SeedEpic2ReviewDataMigration_UsesVariedRatingsAndMatchingToolCaches()
    {
        var migration = File.ReadAllText(GetRepositoryFile("src", "ReviewPortal.Infrastructure", "Migrations", "20260422223500_SeedEpic2ReviewData.cs"));

        Assert.Contains("4.40, N'Approved'", migration);
        Assert.Contains("4.20, N'Approved'", migration);
        Assert.Contains("3.80, N'Approved'", migration);
        Assert.Contains("MERGE dbo.Tools", migration);
        Assert.Contains("(2001, CAST(4.30 AS decimal(3,2)), 2)", migration);
        Assert.Contains("(2002, CAST(4.80 AS decimal(3,2)), 2)", migration);
        Assert.Contains("(2005, CAST(3.90 AS decimal(3,2)), 2)", migration);
        Assert.Contains("(2007, CAST(4.60 AS decimal(3,2)), 1)", migration);
    }

    private static string GetRepositoryFile(params string[] pathSegments)
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var candidate = Path.Combine(new[] { currentDirectory.FullName }.Concat(pathSegments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new FileNotFoundException($"Could not find {Path.Combine(pathSegments)} from {AppContext.BaseDirectory}.");
    }
}
