using System.Text.RegularExpressions;

namespace ReviewPortal.UnitTests.Infrastructure;

public class Epic1SeedTests
{
    [Fact]
    public void FullSeedScript_ProvidesAtLeastThreeCategoriesWithFourCatalogueTools()
    {
        var seedScript = File.ReadAllText(GetRepositoryFile("scripts", "sql", "SeedFullTestData.sql"));
        var toolCategoryMatches = Regex.Matches(seedScript, @"\(\s*20\d{2},\s*(100\d),", RegexOptions.CultureInvariant);

        var categoryCounts = toolCategoryMatches
            .GroupBy(match => match.Groups[1].Value)
            .ToDictionary(group => group.Key, group => group.Count());

        Assert.True(categoryCounts.Values.Count(count => count >= 4) >= 3);
        Assert.True(categoryCounts.Values.Sum() >= 12);
    }

    [Fact]
    public void SeedEpic1CatalogueDataMigration_SeedsCategoriesToolsAndImages()
    {
        var migration = File.ReadAllText(GetRepositoryFile("src", "ReviewPortal.Infrastructure", "Migrations", "20260409234000_SeedEpic1CatalogueData.cs"));

        Assert.Contains("MERGE dbo.Categories", migration);
        Assert.Contains("MERGE dbo.Tools", migration);
        Assert.Contains("MERGE dbo.ToolImages", migration);
        Assert.Contains("(2018, 1003", migration);
        Assert.Contains("(3036, 2018", migration);
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
