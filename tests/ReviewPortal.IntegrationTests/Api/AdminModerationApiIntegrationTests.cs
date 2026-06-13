using System.Net;
using System.Net.Http.Json;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class AdminModerationApiIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public AdminModerationApiIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PendingModerationQueue_WhenReviewsAndCommentsArePending_ReturnsItemLevelRowsAndExactCounts()
    {
        using var customerClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.CustomerEmail,
            ReviewPortalApiFactory.CustomerPassword);
        using var moderatorClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.ModeratorEmail,
            ReviewPortalApiFactory.ModeratorPassword);

        var reviewForComments = await SubmitReviewAsync(
            customerClient,
            "This approved review exists so comment moderation can be tested through the queue.",
            equipmentRating: 5);

        var approveReviewForComments = await moderatorClient.PutAsJsonAsync(
            $"/api/admin/moderation/reviews/{reviewForComments.Id}",
            new ModerateReviewRequest(true, null));
        Assert.Equal(HttpStatusCode.OK, approveReviewForComments.StatusCode);

        var firstPendingComment = await SubmitCommentAsync(
            customerClient,
            reviewForComments.Id,
            "First pending comment for item-level moderation coverage.");
        var secondPendingComment = await SubmitCommentAsync(
            customerClient,
            reviewForComments.Id,
            "Second pending comment for exact queue count coverage.");

        var pendingReview = await SubmitReviewAsync(
            customerClient,
            "This second review should remain pending beside the two pending comments.",
            equipmentRating: 4);

        var firstPage = await moderatorClient.GetFromJsonAsync<PagedList<ModerationQueueItemDto>>(
            "/api/admin/moderation/pending?page=1&pageSize=2");
        var secondPage = await moderatorClient.GetFromJsonAsync<PagedList<ModerationQueueItemDto>>(
            "/api/admin/moderation/pending?page=2&pageSize=2");

        Assert.NotNull(firstPage);
        Assert.NotNull(secondPage);
        Assert.Equal(3, firstPage!.TotalCount);
        Assert.Equal(3, secondPage!.TotalCount);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Single(secondPage.Items);
        Assert.True(firstPage.HasNextPage);
        Assert.True(secondPage.HasPreviousPage);

        var allItems = firstPage.Items.Concat(secondPage.Items).ToList();
        Assert.Equal(3, allItems.Count);
        Assert.All(allItems, item => Assert.Equal(DateTimeKind.Utc, item.SubmittedDate.Kind));

        var reviewItem = Assert.Single(allItems, item => item.ItemType == "Review");
        Assert.Equal(pendingReview.Id, reviewItem.ItemId);
        Assert.Equal(pendingReview.Id, reviewItem.ReviewId);
        Assert.Equal(_factory.TowerScaffoldToolId, reviewItem.ToolId);
        Assert.Equal("API Customer", reviewItem.AuthorName);
        Assert.Equal(4, reviewItem.EquipmentRating);
        Assert.NotNull(reviewItem.OverallRating);

        var commentItems = allItems.Where(item => item.ItemType == "Comment").ToList();
        Assert.Equal(2, commentItems.Count);
        Assert.Contains(commentItems, item => item.ItemId == firstPendingComment.Id && item.ReviewId == reviewForComments.Id);
        Assert.Contains(commentItems, item => item.ItemId == secondPendingComment.Id && item.ReviewId == reviewForComments.Id);
        Assert.All(commentItems, item =>
        {
            Assert.Equal(_factory.TowerScaffoldToolId, item.ToolId);
            Assert.Null(item.EquipmentRating);
            Assert.Null(item.OverallRating);
        });
    }

    [Fact]
    public async Task ModerationEndpoints_WhenRequestInvalidOrItemMissing_ReturnExpectedStatusCodes()
    {
        using var moderatorClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.ModeratorEmail,
            ReviewPortalApiFactory.ModeratorPassword);

        var invalidReject = await moderatorClient.PutAsJsonAsync(
            "/api/admin/moderation/reviews/999999",
            new ModerateReviewRequest(false, null));
        Assert.Equal(HttpStatusCode.BadRequest, invalidReject.StatusCode);

        var missingReview = await moderatorClient.PutAsJsonAsync(
            "/api/admin/moderation/reviews/999999",
            new ModerateReviewRequest(true, null));
        Assert.Equal(HttpStatusCode.NotFound, missingReview.StatusCode);

        var missingComment = await moderatorClient.PutAsJsonAsync(
            "/api/admin/moderation/comments/999999",
            new ModerateReviewRequest(true, null));
        Assert.Equal(HttpStatusCode.NotFound, missingComment.StatusCode);
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
                5,
                5,
                5,
                5));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
        Assert.NotNull(review);
        return review!;
    }

    private static async Task<ReviewCommentDto> SubmitCommentAsync(HttpClient client, int reviewId, string commentText)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/reviews/{reviewId}/comments",
            new CreateCommentRequest(string.Empty, commentText));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var comment = await response.Content.ReadFromJsonAsync<ReviewCommentDto>();
        Assert.NotNull(comment);
        return comment!;
    }
}
