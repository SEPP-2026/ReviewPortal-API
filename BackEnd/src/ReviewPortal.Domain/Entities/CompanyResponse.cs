using ReviewPortal.Domain.Common;

namespace ReviewPortal.Domain.Entities;

public class CompanyResponse : AuditableEntity
{
    public int ReviewId { get; set; }

    public int StaffUserId { get; set; }

    public required string ResponseText { get; set; }

    // Navigation properties
    public Review Review { get; set; } = null!;

    public User StaffUser { get; set; } = null!;
}
