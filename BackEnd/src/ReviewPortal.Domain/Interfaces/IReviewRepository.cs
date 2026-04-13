using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IReadOnlyList<Review>> GetApprovedByToolIdWithDetailsAsync(int toolId, CancellationToken cancellationToken = default);

    Task<Review?> GetByIdWithDetailsAsync(int reviewId, CancellationToken cancellationToken = default);
}
