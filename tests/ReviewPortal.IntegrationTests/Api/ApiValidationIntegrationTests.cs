using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class ApiValidationIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public ApiValidationIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RentalCalculation_WithInvalidDateRange_ReturnsBadRequestValidationProblem()
    {
        var start = new DateTime(2026, 5, 5, 10, 0, 0, DateTimeKind.Utc);
        using var client = _factory.CreateHttpsClient();

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
