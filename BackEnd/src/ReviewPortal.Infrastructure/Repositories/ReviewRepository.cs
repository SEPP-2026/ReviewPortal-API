using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;
using ReviewPortal.Infrastructure.Data;

namespace ReviewPortal.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Review>> GetApprovedByToolIdWithDetailsAsync(
        int toolId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(review => review.ToolId == toolId && review.Status == ReviewStatus.Approved)
            .OrderByDescending(review => review.CreatedDate)
            .ThenByDescending(review => review.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(review => review.Comments.Where(comment => comment.Status == ReviewStatus.Approved))
            .Include(review => review.CompanyResponse)
                .ThenInclude(response => response!.StaffUser)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountApprovedByToolIdAsync(int toolId, CancellationToken cancellationToken = default)
    {
        return _dbSet.CountAsync(
            review => review.ToolId == toolId && review.Status == ReviewStatus.Approved,
            cancellationToken);
    }

    public Task<decimal?> GetAverageOverallRatingByToolIdAsync(int toolId, CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Where(review => review.ToolId == toolId && review.Status == ReviewStatus.Approved)
            .Select(review => (decimal?)review.OverallRating)
            .AverageAsync(cancellationToken);
    }

    public async Task<Review?> GetByIdWithDetailsAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(review => review.Comments)
            .FirstOrDefaultAsync(review => review.Id == reviewId, cancellationToken);
    }
}
