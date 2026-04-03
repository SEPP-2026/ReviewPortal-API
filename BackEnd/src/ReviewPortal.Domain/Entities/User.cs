using ReviewPortal.Domain.Common;
using ReviewPortal.Domain.Enums;

namespace ReviewPortal.Domain.Entities;

public class User : BaseEntity
{
    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();

    public ICollection<CompanyResponse> CompanyResponses { get; set; } = new List<CompanyResponse>();
}
