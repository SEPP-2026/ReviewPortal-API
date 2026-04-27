namespace ReviewPortal.UnitTests.Infrastructure;

public class ReviewRatingConstraintTests
{
    private static readonly string[] ConstraintNames =
    [
        "CK_Reviews_EquipmentRating_Range",
        "CK_Reviews_CustomerServiceRating_Range",
        "CK_Reviews_TechnicalSupportRating_Range",
        "CK_Reviews_AfterSalesRating_Range",
        "CK_Reviews_ValueForMoneyRating_Range"
    ];

    [Fact]
    public void ReviewConfiguration_DefinesDatabaseRatingRangeConstraints()
    {
        var configuration = File.ReadAllText(GetRepositoryFile(
            "src",
            "ReviewPortal.Infrastructure",
            "Configuration",
            "ReviewConfiguration.cs"));

        foreach (var constraintName in ConstraintNames)
        {
            Assert.Contains(constraintName, configuration);
        }

        Assert.Equal(5, CountOccurrences(configuration, "BETWEEN 1 AND 5"));
    }

    [Fact]
    public void AddReviewRatingConstraintsMigration_AddsAllRatingRangeConstraints()
    {
        var migration = File.ReadAllText(GetRepositoryFileByPattern(
            "src",
            "ReviewPortal.Infrastructure",
            "Migrations",
            "*_AddReviewRatingConstraints.cs"));

        foreach (var constraintName in ConstraintNames)
        {
            Assert.Contains(constraintName, migration);
        }

        Assert.Equal(5, CountOccurrences(migration, "AddCheckConstraint"));
        Assert.Equal(5, CountOccurrences(migration, "DropCheckConstraint"));
    }

    [Fact]
    public void AddReviewRatingConstraintsSqlScript_AddsAllRatingRangeConstraints()
    {
        var script = File.ReadAllText(GetRepositoryFile(
            "scripts",
            "sql",
            "AddReviewRatingConstraints.sql"));

        foreach (var constraintName in ConstraintNames)
        {
            Assert.Contains(constraintName, script);
        }

        Assert.Equal(5, CountOccurrences(script, "BETWEEN 1 AND 5"));
    }

    private static int CountOccurrences(string value, string pattern)
    {
        var count = 0;
        var index = 0;

        while ((index = value.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
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

    private static string GetRepositoryFileByPattern(params string[] pathSegments)
    {
        var filePattern = pathSegments[^1];
        var directorySegments = pathSegments[..^1];
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var candidateDirectory = Path.Combine(new[] { currentDirectory.FullName }.Concat(directorySegments).ToArray());
            if (Directory.Exists(candidateDirectory))
            {
                var file = Directory.GetFiles(candidateDirectory, filePattern, SearchOption.TopDirectoryOnly).SingleOrDefault();
                if (file is not null)
                {
                    return file;
                }
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new FileNotFoundException($"Could not find pattern {filePattern} under {Path.Combine(directorySegments)} from {AppContext.BaseDirectory}.");
    }
}
