namespace ReviewPortal.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}
