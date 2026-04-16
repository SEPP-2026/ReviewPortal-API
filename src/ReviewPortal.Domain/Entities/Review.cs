using ReviewPortal.Domain.Common;
using ReviewPortal.Domain.Enums;

namespace ReviewPortal.Domain.Entities;

public class Review : BaseEntity
{
    public int ToolId { get; set; }

    public int? UserId { get; set; }

    public required string ReviewerName { get; set; }

    public required string ReviewerEmail { get; set; }

    public required string ReviewText { get; set; }

    public int EquipmentRating { get; set; }

    public int CustomerServiceRating { get; set; }

    public int TechnicalSupportRating { get; set; }

    public int AfterSalesRating { get; set; }

    public int ValueForMoneyRating { get; set; }

    public decimal OverallRating { get; set; }

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    public string? RejectionReason { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Tool Tool { get; set; } = null!;

    public User? User { get; set; }

    public ICollection<ReviewComment> Comments { get; set; } = new List<ReviewComment>();

    public CompanyResponse? CompanyResponse { get; set; }

    /// <summary>
    /// Calculates the overall rating as the average of the five individual ratings.
    /// </summary>
    public void CalculateOverallRating()
    {
        OverallRating = (EquipmentRating + CustomerServiceRating +
                         TechnicalSupportRating + AfterSalesRating +
                         ValueForMoneyRating) / 5.0m;
    }
}
