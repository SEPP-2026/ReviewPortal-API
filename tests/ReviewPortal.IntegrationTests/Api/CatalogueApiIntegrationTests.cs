using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Categories;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class CatalogueApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public CatalogueApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PublicCatalogueEndpoints_ReturnSeededCategoriesToolsSearchDetailAndRentalCalculation()
    {
        using var client = _factory.CreateHttpsClient();

        var categories = await client.GetFromJsonAsync<CategoryDto[]>("/api/categories");
        Assert.NotNull(categories);
        Assert.Contains(categories!, category => category.Id == _factory.AccessCategoryId && category.Name == "Access & Lifting");

        var categoryTools = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            $"/api/categories/{_factory.AccessCategoryId}/tools?page=1&pageSize=10&sortBy=price&sortOrder=asc");
        Assert.NotNull(categoryTools);
        Assert.Equal(1, categoryTools!.TotalCount);
        Assert.Equal("Tower Scaffold", Assert.Single(categoryTools.Items).Name);

        var searchResults = await client.GetFromJsonAsync<PagedList<ToolSummaryDto>>(
            "/api/tools/search?q=tower&page=1&pageSize=10");
        Assert.NotNull(searchResults);
        Assert.Contains(searchResults!.Items, tool => tool.Id == _factory.TowerScaffoldToolId && tool.Name == "Tower Scaffold");

        var detail = await client.GetFromJsonAsync<ToolDto>($"/api/tools/{_factory.TowerScaffoldToolId}");
        Assert.NotNull(detail);
        Assert.Equal("Tower Scaffold", detail!.Name);
        Assert.True(detail.HasEnoughReviewsToRate);
        Assert.Equal(2, detail.ReviewCount);
        Assert.NotNull(detail.OverallRating);
        Assert.Equal("/uploads/tools/tower-scaffold.jpg", Assert.Single(detail.Images).ImageUrl);

        var start = new DateTime(2026, 5, 5, 9, 0, 0, DateTimeKind.Utc);
        var rentalResponse = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/rental-calculation",
            new RentalCalculationRequest(start, start.AddDays(1)));

        Assert.Equal(HttpStatusCode.OK, rentalResponse.StatusCode);
        var rental = await rentalResponse.Content.ReadFromJsonAsync<RentalCalculationResponse>();
        Assert.NotNull(rental);
        Assert.Equal("Tower Scaffold", rental!.ToolName);
        Assert.Equal(45m, rental.TotalCost);
        Assert.Contains("1 day x 45.00/day", rental.Breakdown);
    }
}
