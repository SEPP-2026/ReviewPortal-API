using ReviewPortal.Domain.Common;

namespace ReviewPortal.Domain.Entities;

public class ToolImage : BaseEntity
{
    public int ToolId { get; set; }

    public required string ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Tool Tool { get; set; } = null!;
}
