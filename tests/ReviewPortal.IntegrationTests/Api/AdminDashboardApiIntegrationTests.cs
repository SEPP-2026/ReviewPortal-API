using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.DTOs.Dashboard;
using ReviewPortal.Application.DTOs.Reviews;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class AdminDashboardApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public AdminDashboardApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AdminDashboardStats_WhenBackendDataChanges_ReturnsAccurateEpic3Overview()
    {
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);
        using var customerClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.CustomerEmail,
            ReviewPortalApiFactory.CustomerPassword);

        var initialStats = await adminClient.GetFromJsonAsync<DashboardStatsDto>("/api/admin/dashboard/stats");
        Assert.NotNull(initialStats);
        Assert.Equal(2, initialStats!.TotalActiveTools);
        Assert.Equal(1, initialStats.TotalInactiveTools);
        Assert.Equal(0, initialStats.PendingModerationCount);
        Assert.Contains(initialStats.TopRatedTools, tool =>
            tool.ToolId == _factory.TowerScaffoldToolId &&
            tool.ReviewCount == 2 &&
            tool.OverallRating == 4.4m);
        Assert.Contains(initialStats.MostReviewedTools, tool =>
            tool.ToolId == _factory.TowerScaffoldToolId &&
            tool.ReviewCount == 2);

        var pendingReview = await SubmitReviewAsync(customerClient);

        var statsWithPendingReview = await adminClient.GetFromJsonAsync<DashboardStatsDto>("/api/admin/dashboard/stats");
        Assert.NotNull(statsWithPendingReview);
        Assert.Equal(initialStats.ReviewsPublishedThisMonth, statsWithPendingReview!.ReviewsPublishedThisMonth);
        Assert.Equal(1, statsWithPendingReview.PendingModerationCount);

        var approval = await adminClient.PutAsJsonAsync(
            $"/api/admin/moderation/reviews/{pendingReview.Id}",
            new ModerateReviewRequest(true, null));
        Assert.Equal(HttpStatusCode.OK, approval.StatusCode);

        var statsAfterApproval = await adminClient.GetFromJsonAsync<DashboardStatsDto>("/api/admin/dashboard/stats");
        Assert.NotNull(statsAfterApproval);
        Assert.Equal(0, statsAfterApproval!.PendingModerationCount);
        Assert.Equal(initialStats.ReviewsPublishedThisMonth + 1, statsAfterApproval.ReviewsPublishedThisMonth);
        Assert.Contains(statsAfterApproval.TopRatedTools, tool =>
            tool.ToolId == _factory.TowerScaffoldToolId &&
            tool.ReviewCount == 3 &&
            tool.OverallRating == 4.6m);
        Assert.Contains(statsAfterApproval.MostReviewedTools, tool =>
            tool.ToolId == _factory.TowerScaffoldToolId &&
            tool.ReviewCount == 3);

        var pendingComment = await customerClient.PostAsJsonAsync(
            $"/api/reviews/{pendingReview.Id}/comments",
            new CreateCommentRequest(
                string.Empty,
                "Pending dashboard comment count should include review comments."));
        Assert.Equal(HttpStatusCode.Created, pendingComment.StatusCode);

        var statsWithPendingComment = await adminClient.GetFromJsonAsync<DashboardStatsDto>("/api/admin/dashboard/stats");
        Assert.NotNull(statsWithPendingComment);
        Assert.Equal(1, statsWithPendingComment!.PendingModerationCount);
        Assert.Equal(2, statsWithPendingComment.TotalActiveTools);
        Assert.Equal(1, statsWithPendingComment.TotalInactiveTools);
    }

    private async Task<ReviewDto> SubmitReviewAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews",
            new CreateReviewRequest(
                string.Empty,
                string.Empty,
                "This dashboard integration review is approved in the current month for reporting coverage.",
                5,
                5,
                5,
                5,
                5));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
        Assert.NotNull(review);
        return review!;
    }
}
