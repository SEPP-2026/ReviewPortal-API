using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.IntegrationTests.Api;

public class ApiValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiValidationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        SetStartupConfiguration();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=ReviewPortalValidationTests;Trusted_Connection=True;TrustServerCertificate=True;",
                    ["Jwt:Secret"] = "ValidationTestsUseASecretWithAtLeastThirtyTwoChars",
                    ["Jwt:Issuer"] = "ReviewPortal.Tests",
                    ["Jwt:Audience"] = "ReviewPortal.Tests",
                    ["Jwt:ExpiryMinutes"] = "60",
                    ["ImageStorage:RootPath"] = "TestUploads",
                    ["ImageStorage:RequestPath"] = "/uploads/tools"
                });
            });
        });
    }

    private static void SetStartupConfiguration()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(localdb)\\MSSQLLocalDB;Database=ReviewPortalValidationTests;Trusted_Connection=True;TrustServerCertificate=True;");
        Environment.SetEnvironmentVariable("Jwt__Secret", "ValidationTestsUseASecretWithAtLeastThirtyTwoChars");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "ReviewPortal.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "ReviewPortal.Tests");
        Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", "60");
        Environment.SetEnvironmentVariable("ImageStorage__RootPath", "TestUploads");
        Environment.SetEnvironmentVariable("ImageStorage__RequestPath", "/uploads/tools");
    }

    [Fact]
    public async Task RentalCalculation_WithInvalidDateRange_ReturnsBadRequestValidationProblem()
    {
        var start = new DateTime(2026, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync(
            "/api/tools/1/rental-calculation",
            new RentalCalculationRequest(start, start.AddHours(-1)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey("EndDateTime"));
        Assert.Contains(
            "End date/time must be after the start date/time.",
            problem.Errors["EndDateTime"]);
    }
}
