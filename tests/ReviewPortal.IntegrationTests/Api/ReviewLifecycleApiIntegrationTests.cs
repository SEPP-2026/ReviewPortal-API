using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class ReviewLifecycleApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public ReviewLifecycleApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ReviewLifecycle_SubmitApproveRejectAndVisibilityFlowsThroughHttpPipeline()
    {
        using var customerClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.CustomerEmail,
            ReviewPortalApiFactory.CustomerPassword);
        using var moderatorClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.ModeratorEmail,
            ReviewPortalApiFactory.ModeratorPassword);

        var approvedReview = await SubmitReviewAsync(
            customerClient,
            "This tower scaffold hire was smooth, safe, and exactly what the site team needed.",
            equipmentRating: 5);
        Assert.Equal("Pending", approvedReview.Status);

        var myPendingReviews = await customerClient.GetFromJsonAsync<PagedList<ReviewSummaryDto>>("/api/users/me/reviews");
        Assert.NotNull(myPendingReviews);
        Assert.Contains(myPendingReviews!.Items, review => review.Id == approvedReview.Id && review.Status == "Pending");

        var pendingQueue = await moderatorClient.GetFromJsonAsync<PagedList<ReviewDto>>("/api/admin/moderation/pending?page=1&pageSize=10");
        Assert.NotNull(pendingQueue);
        Assert.Contains(pendingQueue!.Items, review => review.Id == approvedReview.Id);

        var approval = await moderatorClient.PutAsJsonAsync(
            $"/api/admin/moderation/reviews/{approvedReview.Id}",
            new ModerateReviewRequest(true, null));
        Assert.Equal(HttpStatusCode.OK, approval.StatusCode);

        var approvedList = await customerClient.GetFromJsonAsync<ToolReviewsDto>(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews?page=1&pageSize=10");
        Assert.NotNull(approvedList);
        Assert.Contains(approvedList!.Reviews.Items, review => review.Id == approvedReview.Id && review.Status == "Approved");
        Assert.Equal(3, approvedList.TotalApprovedReviews);

        var rejectedReview = await SubmitReviewAsync(
            customerClient,
            "This second review is intentionally submitted for rejection coverage in the API suite.",
            equipmentRating: 3);

        var rejection = await moderatorClient.PutAsJsonAsync(
            $"/api/admin/moderation/reviews/{rejectedReview.Id}",
            new ModerateReviewRequest(false, "Duplicate submission for integration coverage."));
        Assert.Equal(HttpStatusCode.OK, rejection.StatusCode);

        var myFinalReviews = await customerClient.GetFromJsonAsync<PagedList<ReviewSummaryDto>>("/api/users/me/reviews?page=1&pageSize=10");
        Assert.NotNull(myFinalReviews);
        Assert.Contains(
            myFinalReviews!.Items,
            review => review.Id == rejectedReview.Id &&
                      review.Status == "Rejected" &&
                      review.RejectionReason == "Duplicate submission for integration coverage.");

        var approvedListAfterRejection = await customerClient.GetFromJsonAsync<ToolReviewsDto>(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews?page=1&pageSize=10");
        Assert.NotNull(approvedListAfterRejection);
        Assert.DoesNotContain(approvedListAfterRejection!.Reviews.Items, review => review.Id == rejectedReview.Id);
    }

    private async Task<ReviewDto> SubmitReviewAsync(HttpClient client, string reviewText, int equipmentRating)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews",
            new CreateReviewRequest(
                string.Empty,
                string.Empty,
                reviewText,
                equipmentRating,
                4,
                5,
                4,
                5));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
        Assert.NotNull(review);
        return review!;
    }
}
