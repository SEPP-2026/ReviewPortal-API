namespace ReviewPortal.UnitTests.Infrastructure;

public class AuthSqlScriptTests
{
    [Fact]
    public void PasswordResetSchemaScript_IsIdempotentAndAddsExpectedColumns()
    {
        var script = File.ReadAllText(GetRepositoryFile("scripts", "sql", "AddUserPasswordResetFields.sql"));

        Assert.Contains("COL_LENGTH('dbo.Users', 'PasswordResetTokenHash') IS NULL", script);
        Assert.Contains("COL_LENGTH('dbo.Users', 'PasswordResetTokenExpiryUtc') IS NULL", script);
        Assert.Contains("ALTER TABLE dbo.Users", script);
        Assert.Contains("PasswordResetTokenHash nvarchar(256) NULL", script);
        Assert.Contains("PasswordResetTokenExpiryUtc datetime2 NULL", script);
    }

    [Fact]
    public void UserSeedScripts_ClearPasswordResetColumnsWhenSchemaSupportsThem()
    {
        var testUsersSeed = File.ReadAllText(GetRepositoryFile("scripts", "sql", "SeedTestUsers.sql"));
        var fullSeed = File.ReadAllText(GetRepositoryFile("scripts", "sql", "SeedFullTestData.sql"));

        Assert.Contains("@HasPasswordResetColumns", testUsersSeed);
        Assert.Contains("PasswordResetTokenHash = NULL", testUsersSeed);
        Assert.Contains("PasswordResetTokenExpiryUtc = NULL", testUsersSeed);

        Assert.Contains("@HasPasswordResetColumns", fullSeed);
        Assert.Contains("PasswordResetTokenHash", fullSeed);
        Assert.Contains("PasswordResetTokenExpiryUtc", fullSeed);
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
