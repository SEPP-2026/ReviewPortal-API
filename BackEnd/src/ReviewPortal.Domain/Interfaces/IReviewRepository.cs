using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IReadOnlyList<Review>> GetApprovedByToolIdWithDetailsAsync(
        int toolId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountApprovedByToolIdAsync(int toolId, CancellationToken cancellationToken = default);

    Task<decimal?> GetAverageOverallRatingByToolIdAsync(int toolId, CancellationToken cancellationToken = default);

    Task<Review?> GetByIdWithDetailsAsync(int reviewId, CancellationToken cancellationToken = default);
}
