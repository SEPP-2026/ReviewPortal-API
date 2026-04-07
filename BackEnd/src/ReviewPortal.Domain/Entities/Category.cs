using ReviewPortal.Domain.Common;

namespace ReviewPortal.Domain.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    // Navigation properties
    public ICollection<Tool> Tools { get; set; } = new List<Tool>();
}
