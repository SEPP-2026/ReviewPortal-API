using ReviewPortal.Domain.Common;

namespace ReviewPortal.Domain.Entities;

public class Tool : AuditableEntity
{
    public int CategoryId { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public decimal HourlyRate { get; set; }

    public decimal DailyRate { get; set; }

    public decimal WeeklyRate { get; set; }

    public string? SpecialNotes { get; set; }

    public bool DepositRequired { get; set; }

    public decimal? DepositAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal? OverallRating { get; set; }

    public int ReviewCount { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;

    public ICollection<ToolImage> Images { get; set; } = new List<ToolImage>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
