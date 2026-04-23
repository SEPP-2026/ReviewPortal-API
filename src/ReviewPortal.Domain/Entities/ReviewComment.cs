using ReviewPortal.Domain.Common;
using ReviewPortal.Domain.Enums;

namespace ReviewPortal.Domain.Entities;

public class ReviewComment : BaseEntity
{
    public int ReviewId { get; set; }

    public int? UserId { get; set; }

    public required string CommenterName { get; set; }

    public required string CommentText { get; set; }

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    public string? RejectionReason { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Review Review { get; set; } = null!;

    public User? User { get; set; }
}
