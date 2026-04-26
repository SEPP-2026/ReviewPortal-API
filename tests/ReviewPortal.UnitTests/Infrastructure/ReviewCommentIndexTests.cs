namespace ReviewPortal.UnitTests.Infrastructure;

public class ReviewCommentIndexTests
{
    [Fact]
    public void ReviewCommentConfiguration_DefinesStatusIndex()
    {
        var configuration = File.ReadAllText(GetRepositoryFile(
            "src",
            "ReviewPortal.Infrastructure",
            "Configuration",
            "ReviewCommentConfiguration.cs"));

        Assert.Contains("builder.HasIndex(rc => rc.Status);", configuration);
    }

    [Fact]
    public void AddReviewCommentsStatusIndexMigration_CreatesReviewCommentsStatusIndex()
    {
        var migration = File.ReadAllText(GetRepositoryFileByPattern(
            "src",
            "ReviewPortal.Infrastructure",
            "Migrations",
            "*_AddReviewCommentsStatusIndex.cs"));

        Assert.Contains("name: \"IX_ReviewComments_Status\"", migration);
        Assert.Contains("table: \"ReviewComments\"", migration);
        Assert.Contains("column: \"Status\"", migration);
    }

    [Fact]
    public void AddReviewCommentsStatusIndexSqlScript_CreatesReviewCommentsStatusIndex()
    {
        var script = File.ReadAllText(GetRepositoryFile(
            "scripts",
            "sql",
            "AddReviewCommentsStatusIndex.sql"));

        Assert.Contains("IX_ReviewComments_Status", script);
        Assert.Contains("[ReviewComments]", script);
        Assert.Contains("[Status]", script);
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
