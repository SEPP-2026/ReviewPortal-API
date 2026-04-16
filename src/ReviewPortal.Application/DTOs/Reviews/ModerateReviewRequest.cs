namespace ReviewPortal.Application.DTOs.Reviews;

public record ModerateReviewRequest(
    bool Approved,
    string? RejectionReason
);
