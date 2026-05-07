using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Reviews;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Infrastructure.Data;

namespace ReviewPortal.IntegrationTests.Api;

[Collection(ReviewPortalApiCollection.CollectionName)]
public class Epic2ApiContractIntegrationTests : IAsyncLifetime
{
    private readonly ReviewPortalApiFactory _factory;

    public Epic2ApiContractIntegrationTests(ReviewPortalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateReview_WhenAnonymousOrAuthenticated_StoresPendingAndUsesCorrectReviewerIdentity()
    {
        using var anonymousClient = _factory.CreateHttpsClient();
        using var customerClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.CustomerEmail,
            ReviewPortalApiFactory.CustomerPassword);

        var anonymousReview = await SubmitReviewAsync(
            anonymousClient,
            "Anonymous customer feedback with enough detail for the review contract.",
            reviewerName: "Nina Visitor",
            reviewerEmail: "nina.visitor@example.com",
            equipmentRating: 5,
            customerServiceRating: 4,
            technicalSupportRating: 4,
            afterSalesRating: 5,
            valueForMoneyRating: 3);

        Assert.Equal("Pending", anonymousReview.Status);
        Assert.Equal("Nina Visitor", anonymousReview.ReviewerName);
        Assert.Equal(4.2m, anonymousReview.OverallRating);

        var authenticatedReview = await SubmitReviewAsync(
            customerClient,
            "Authenticated customer feedback should use the account identity from the JWT.",
            reviewerName: "Spoofed Name",
            reviewerEmail: "spoofed@example.com");

        Assert.Equal("Pending", authenticatedReview.Status);
        Assert.Equal("API Customer", authenticatedReview.ReviewerName);

        var publicReviews = await anonymousClient.GetFromJsonAsync<ToolReviewsDto>(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews?page=1&pageSize=10");

        Assert.NotNull(publicReviews);
        Assert.Equal(2, publicReviews!.TotalApprovedReviews);
        Assert.DoesNotContain(publicReviews.Reviews.Items, review => review.Id == anonymousReview.Id);
        Assert.DoesNotContain(publicReviews.Reviews.Items, review => review.Id == authenticatedReview.Id);
    }

    [Fact]
    public async Task CreateReview_WhenInvalidOrToolUnavailable_ReturnsBadRequestOrNotFound()
    {
        using var client = _factory.CreateHttpsClient();

        var invalidRating = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews",
            new CreateReviewRequest(
                "Alex Visitor",
                "alex.visitor@example.com",
                "This review has enough text but an invalid equipment rating.",
                0,
                4,
                4,
                4,
                4));
        Assert.Equal(HttpStatusCode.BadRequest, invalidRating.StatusCode);

        var tooShortText = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews",
            new CreateReviewRequest(
                "Alex Visitor",
                "alex.visitor@example.com",
                "Too short",
                4,
                4,
                4,
                4,
                4));
        Assert.Equal(HttpStatusCode.BadRequest, tooShortText.StatusCode);

        var missingTool = await client.PostAsJsonAsync(
            "/api/tools/999999/reviews",
            new CreateReviewRequest(
                "Alex Visitor",
                "alex.visitor@example.com",
                "This review targets a missing tool and should return not found.",
                4,
                4,
                4,
                4,
                4));
        Assert.Equal(HttpStatusCode.NotFound, missingTool.StatusCode);

        var inactiveTool = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.RetiredLadderToolId}/reviews",
            new CreateReviewRequest(
                "Alex Visitor",
                "alex.visitor@example.com",
                "This review targets an inactive tool and should return not found.",
                4,
                4,
                4,
                4,
                4));
        Assert.Equal(HttpStatusCode.NotFound, inactiveTool.StatusCode);
    }

    [Fact]
    public async Task GetApprovedReviews_ReturnsNewestFirstPaginationAverageEmptyStateAndExcludesRejected()
    {
        using var client = _factory.CreateHttpsClient();
        var newestApprovedId = await AddReviewAsync(
            "Newest Approved",
            ReviewStatus.Approved,
            new DateTime(2026, 4, 4, 10, 0, 0, DateTimeKind.Utc),
            equipmentRating: 5,
            customerServiceRating: 5,
            technicalSupportRating: 5,
            afterSalesRating: 5,
            valueForMoneyRating: 5);
        await AddReviewAsync(
            "Rejected Later",
            ReviewStatus.Rejected,
            new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc),
            rejectionReason: "Rejected reviews must not appear publicly.");

        var firstPage = await client.GetFromJsonAsync<ToolReviewsDto>(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews?page=1&pageSize=2");
        var secondPage = await client.GetFromJsonAsync<ToolReviewsDto>(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews?page=2&pageSize=2");

        Assert.NotNull(firstPage);
        Assert.NotNull(secondPage);
        Assert.Equal(3, firstPage!.TotalApprovedReviews);
        Assert.Equal(4.6m, firstPage.AverageOverallRating);
        Assert.Equal(3, firstPage.Reviews.TotalCount);
        Assert.Equal(2, firstPage.Reviews.Items.Count);
        Assert.True(firstPage.Reviews.HasNextPage);
        Assert.Single(secondPage!.Reviews.Items);
        Assert.True(secondPage.Reviews.HasPreviousPage);

        Assert.Equal(newestApprovedId, firstPage.Reviews.Items[0].Id);
        Assert.Equal("Morgan Build", firstPage.Reviews.Items[1].ReviewerName);
        Assert.Equal("Alex Site", secondPage.Reviews.Items[0].ReviewerName);
        Assert.DoesNotContain(firstPage.Reviews.Items.Concat(secondPage.Reviews.Items), review => review.ReviewerName == "Rejected Later");

        var empty = await client.GetFromJsonAsync<ToolReviewsDto>(
            $"/api/tools/{_factory.RotaryHammerToolId}/reviews?page=1&pageSize=10");

        Assert.NotNull(empty);
        Assert.Null(empty!.AverageOverallRating);
        Assert.Equal(0, empty.TotalApprovedReviews);
        Assert.Empty(empty.Reviews.Items);
        Assert.Contains("No reviews", empty.EmptyStateMessage);
    }

    [Fact]
    public async Task Comments_WhenSubmittedOrRejected_RequireModerationAndPublicListOnlyApproved()
    {
        using var client = _factory.CreateHttpsClient();
        using var moderatorClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.ModeratorEmail,
            ReviewPortalApiFactory.ModeratorPassword);
        var reviewId = await GetLatestApprovedTowerReviewIdAsync();
        var seededComments = await AddCommentsAsync(reviewId);

        var pendingComment = await SubmitCommentAsync(
            client,
            reviewId,
            "Nina Visitor",
            "This customer comment should enter moderation before it appears publicly.");

        Assert.Equal("Pending", pendingComment.Status);

        var publicBeforeApproval = await client.GetFromJsonAsync<ReviewCommentDto[]>($"/api/reviews/{reviewId}/comments");
        Assert.NotNull(publicBeforeApproval);
        Assert.Contains(publicBeforeApproval!, comment => comment.Id == seededComments.ApprovedCommentId);
        Assert.DoesNotContain(publicBeforeApproval!, comment => comment.Id == pendingComment.Id);
        Assert.DoesNotContain(publicBeforeApproval!, comment => comment.Id == seededComments.RejectedCommentId);

        var approveComment = await moderatorClient.PutAsJsonAsync(
            $"/api/admin/moderation/comments/{pendingComment.Id}",
            new ModerateReviewRequest(true, null));
        Assert.Equal(HttpStatusCode.OK, approveComment.StatusCode);

        var publicAfterApproval = await client.GetFromJsonAsync<ReviewCommentDto[]>($"/api/reviews/{reviewId}/comments");
        Assert.NotNull(publicAfterApproval);
        Assert.Contains(publicAfterApproval!, comment => comment.Id == pendingComment.Id && comment.Status == "Approved");
        Assert.DoesNotContain(publicAfterApproval!, comment => comment.Id == seededComments.RejectedCommentId);

        var invalidComment = await client.PostAsJsonAsync(
            $"/api/reviews/{reviewId}/comments",
            new CreateCommentRequest("Nina Visitor", "Short"));
        Assert.Equal(HttpStatusCode.BadRequest, invalidComment.StatusCode);

        var missingReview = await client.PostAsJsonAsync(
            "/api/reviews/999999/comments",
            new CreateCommentRequest("Nina Visitor", "This comment targets a review that does not exist."));
        Assert.Equal(HttpStatusCode.NotFound, missingReview.StatusCode);
    }

    [Fact]
    public async Task CompanyResponses_UseStaffPolicyAndApprovedReviewRulesThroughFullLifecycle()
    {
        using var anonymousClient = _factory.CreateHttpsClient();
        using var customerClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.CustomerEmail,
            ReviewPortalApiFactory.CustomerPassword);
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);
        using var moderatorClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.ModeratorEmail,
            ReviewPortalApiFactory.ModeratorPassword);

        var approvedReviewId = await GetLatestApprovedTowerReviewIdAsync();
        var pendingReview = await SubmitReviewAsync(
            customerClient,
            "Pending review for proving official company responses only attach after approval.");

        var customerForbidden = await customerClient.PostAsJsonAsync(
            $"/api/reviews/{approvedReviewId}/response",
            new CreateCompanyResponseRequest("Customer users cannot publish official responses."));
        Assert.Equal(HttpStatusCode.Forbidden, customerForbidden.StatusCode);

        var pendingReviewGuard = await adminClient.PostAsJsonAsync(
            $"/api/reviews/{pendingReview.Id}/response",
            new CreateCompanyResponseRequest("Staff response on a pending review should be blocked."));
        Assert.Equal(HttpStatusCode.BadRequest, pendingReviewGuard.StatusCode);

        var missingReview = await adminClient.PostAsJsonAsync(
            "/api/reviews/999999/response",
            new CreateCompanyResponseRequest("This response targets a missing review."));
        Assert.Equal(HttpStatusCode.NotFound, missingReview.StatusCode);

        var create = await moderatorClient.PostAsJsonAsync(
            $"/api/reviews/{approvedReviewId}/response",
            new CreateCompanyResponseRequest("Official response created by moderator for contract coverage."));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<CompanyResponseDto>();
        Assert.NotNull(created);
        Assert.Equal("API Moderator", created!.StaffName);

        var duplicate = await adminClient.PostAsJsonAsync(
            $"/api/reviews/{approvedReviewId}/response",
            new CreateCompanyResponseRequest("A second official response should be rejected."));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var update = await adminClient.PutAsJsonAsync(
            $"/api/reviews/{approvedReviewId}/response",
            new CreateCompanyResponseRequest("Official response updated by admin for public display."));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var updated = await update.Content.ReadFromJsonAsync<CompanyResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal("Official response updated by admin for public display.", updated!.ResponseText);
        Assert.Equal("API Admin", updated.StaffName);

        var publicReviews = await anonymousClient.GetFromJsonAsync<ToolReviewsDto>(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews?page=1&pageSize=10");
        Assert.NotNull(publicReviews);
        var reviewWithResponse = Assert.Single(publicReviews!.Reviews.Items, review => review.Id == approvedReviewId);
        Assert.NotNull(reviewWithResponse.CompanyResponse);
        Assert.Equal(updated.ResponseText, reviewWithResponse.CompanyResponse!.ResponseText);

        var delete = await adminClient.DeleteAsync($"/api/reviews/{approvedReviewId}/response");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var publicAfterDelete = await anonymousClient.GetFromJsonAsync<ToolReviewsDto>(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews?page=1&pageSize=10");
        Assert.NotNull(publicAfterDelete);
        var reviewAfterDelete = Assert.Single(publicAfterDelete!.Reviews.Items, review => review.Id == approvedReviewId);
        Assert.Null(reviewAfterDelete.CompanyResponse);

        var updateMissingResponse = await adminClient.PutAsJsonAsync(
            $"/api/reviews/{approvedReviewId}/response",
            new CreateCompanyResponseRequest("Cannot update a response after it has been deleted."));
        Assert.Equal(HttpStatusCode.NotFound, updateMissingResponse.StatusCode);

        var deleteMissingResponse = await adminClient.DeleteAsync($"/api/reviews/{approvedReviewId}/response");
        Assert.Equal(HttpStatusCode.NotFound, deleteMissingResponse.StatusCode);
    }

    [Fact]
    public async Task GetMyReviews_ReturnsStatusesRejectionReasonResponseFlagPaginationAndRequiresToken()
    {
        using var anonymousClient = _factory.CreateHttpsClient();
        using var customerClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.CustomerEmail,
            ReviewPortalApiFactory.CustomerPassword);
        using var adminClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.AdminEmail,
            ReviewPortalApiFactory.AdminPassword);
        using var moderatorClient = await _factory.CreateAuthenticatedClientAsync(
            ReviewPortalApiFactory.ModeratorEmail,
            ReviewPortalApiFactory.ModeratorPassword);

        var approvedReview = await SubmitReviewAsync(
            customerClient,
            "Customer review that will be approved and receive an official response.");
        var rejectedReview = await SubmitReviewAsync(
            customerClient,
            "Customer review that will be rejected with a clear moderation reason.");
        var pendingReview = await SubmitReviewAsync(
            customerClient,
            "Customer review that should remain pending for the My Reviews page.");

        await ApproveReviewAsync(moderatorClient, approvedReview.Id);
        await RejectReviewAsync(moderatorClient, rejectedReview.Id, "Contains duplicate or irrelevant content.");

        var companyResponse = await adminClient.PostAsJsonAsync(
            $"/api/reviews/{approvedReview.Id}/response",
            new CreateCompanyResponseRequest("Official response linked to the customer's approved review."));
        Assert.Equal(HttpStatusCode.Created, companyResponse.StatusCode);

        var unauthorized = await anonymousClient.GetAsync("/api/users/me/reviews");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var firstPage = await customerClient.GetFromJsonAsync<PagedList<ReviewSummaryDto>>(
            "/api/users/me/reviews?page=1&pageSize=2");
        var secondPage = await customerClient.GetFromJsonAsync<PagedList<ReviewSummaryDto>>(
            "/api/users/me/reviews?page=2&pageSize=2");

        Assert.NotNull(firstPage);
        Assert.NotNull(secondPage);
        Assert.Equal(3, firstPage!.TotalCount);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.True(firstPage.HasNextPage);
        Assert.Single(secondPage!.Items);
        Assert.True(secondPage.HasPreviousPage);

        var allReviews = firstPage.Items.Concat(secondPage.Items).ToList();
        Assert.Contains(allReviews, review => review.Id == approvedReview.Id && review.Status == "Approved" && review.HasCompanyResponse);
        Assert.Contains(allReviews, review => review.Id == rejectedReview.Id && review.Status == "Rejected" && review.RejectionReason == "Contains duplicate or irrelevant content.");
        Assert.Contains(allReviews, review => review.Id == pendingReview.Id && review.Status == "Pending" && review.RejectionReason is null);
    }

    private async Task<ReviewDto> SubmitReviewAsync(
        HttpClient client,
        string reviewText,
        string reviewerName = "",
        string reviewerEmail = "",
        int equipmentRating = 5,
        int customerServiceRating = 4,
        int technicalSupportRating = 5,
        int afterSalesRating = 4,
        int valueForMoneyRating = 5)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/tools/{_factory.TowerScaffoldToolId}/reviews",
            new CreateReviewRequest(
                reviewerName,
                reviewerEmail,
                reviewText,
                equipmentRating,
                customerServiceRating,
                technicalSupportRating,
                afterSalesRating,
                valueForMoneyRating));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
        Assert.NotNull(review);
        return review!;
    }

    private static async Task<ReviewCommentDto> SubmitCommentAsync(
        HttpClient client,
        int reviewId,
        string commenterName,
        string commentText)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/reviews/{reviewId}/comments",
            new CreateCommentRequest(commenterName, commentText));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var comment = await response.Content.ReadFromJsonAsync<ReviewCommentDto>();
        Assert.NotNull(comment);
        return comment!;
    }

    private static async Task ApproveReviewAsync(HttpClient moderatorClient, int reviewId)
    {
        var response = await moderatorClient.PutAsJsonAsync(
            $"/api/admin/moderation/reviews/{reviewId}",
            new ModerateReviewRequest(true, null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task RejectReviewAsync(HttpClient moderatorClient, int reviewId, string rejectionReason)
    {
        var response = await moderatorClient.PutAsJsonAsync(
            $"/api/admin/moderation/reviews/{reviewId}",
            new ModerateReviewRequest(false, rejectionReason));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<int> GetLatestApprovedTowerReviewIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Reviews
            .Where(review => review.ToolId == _factory.TowerScaffoldToolId && review.Status == ReviewStatus.Approved)
            .OrderByDescending(review => review.CreatedDate)
            .ThenByDescending(review => review.Id)
            .Select(review => review.Id)
            .FirstAsync();
    }

    private async Task<int> AddReviewAsync(
        string reviewerName,
        ReviewStatus status,
        DateTime createdDate,
        int equipmentRating = 4,
        int customerServiceRating = 4,
        int technicalSupportRating = 4,
        int afterSalesRating = 4,
        int valueForMoneyRating = 4,
        string? rejectionReason = null)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var review = new Review
        {
            ToolId = _factory.TowerScaffoldToolId,
            ReviewerName = reviewerName,
            ReviewerEmail = $"{reviewerName.Replace(" ", ".", StringComparison.OrdinalIgnoreCase).ToLowerInvariant()}@example.com",
            ReviewText = $"{reviewerName} submitted detailed feedback for Epic 2 contract coverage.",
            EquipmentRating = equipmentRating,
            CustomerServiceRating = customerServiceRating,
            TechnicalSupportRating = technicalSupportRating,
            AfterSalesRating = afterSalesRating,
            ValueForMoneyRating = valueForMoneyRating,
            Status = status,
            RejectionReason = rejectionReason,
            CreatedDate = createdDate
        };
        review.CalculateOverallRating();

        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        return review.Id;
    }

    private async Task<(int ApprovedCommentId, int RejectedCommentId)> AddCommentsAsync(int reviewId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var approvedComment = new ReviewComment
        {
            ReviewId = reviewId,
            CommenterName = "Approved Commenter",
            CommentText = "Already approved public comment for visibility coverage.",
            Status = ReviewStatus.Approved,
            CreatedDate = new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc)
        };
        var rejectedComment = new ReviewComment
        {
            ReviewId = reviewId,
            CommenterName = "Rejected Commenter",
            CommentText = "Rejected comment should remain hidden from the public endpoint.",
            Status = ReviewStatus.Rejected,
            RejectionReason = "Irrelevant to the hire experience.",
            CreatedDate = new DateTime(2026, 4, 6, 10, 0, 0, DateTimeKind.Utc)
        };

        context.ReviewComments.AddRange(approvedComment, rejectedComment);
        await context.SaveChangesAsync();

        return (approvedComment.Id, rejectedComment.Id);
    }
}
