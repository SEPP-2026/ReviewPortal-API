using System.Net;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class SwaggerApiIntegrationTests
{
    private readonly ReviewPortalApiFactory _factory;

    public SwaggerApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerJson_ReturnsOpenApiDocument()
    {
        using var client = _factory.CreateHttpsClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"openapi\"", body);
        Assert.Contains("ReviewPortal.API", body);
        Assert.Contains("multipart/form-data", body);
    }
}
