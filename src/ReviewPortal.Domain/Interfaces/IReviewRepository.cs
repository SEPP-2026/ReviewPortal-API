using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IReadOnlyList<Review>> GetByUserIdWithDetailsAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetApprovedByToolIdWithDetailsAsync(
        int toolId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountApprovedByToolIdAsync(int toolId, CancellationToken cancellationToken = default);

    Task<decimal?> GetAverageOverallRatingByToolIdAsync(int toolId, CancellationToken cancellationToken = default);

    Task<Review?> GetByIdWithDetailsAsync(int reviewId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetPendingWithDetailsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountPendingAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetByToolIdAsync(int toolId, CancellationToken cancellationToken = default);
}
