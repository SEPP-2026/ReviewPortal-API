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

    public async Task<IReadOnlyList<Review>> GetByUserIdWithDetailsAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(review => review.UserId == userId)
            .OrderByDescending(review => review.CreatedDate)
            .ThenByDescending(review => review.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(review => review.Tool)
            .Include(review => review.CompanyResponse)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbSet.CountAsync(review => review.UserId == userId, cancellationToken);
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
            .AsSplitQuery()
            .Include(review => review.Comments)
            .Include(review => review.CompanyResponse)
                .ThenInclude(response => response!.StaffUser)
            .FirstOrDefaultAsync(review => review.Id == reviewId, cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetPendingWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Where(review =>
                review.Status == ReviewStatus.Pending ||
                review.Comments.Any(comment => comment.Status == ReviewStatus.Pending))
            .OrderBy(review => review.Id)
            .Include(review => review.Tool)
            .Include(review => review.Comments.Where(comment => comment.Status == ReviewStatus.Pending))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPendingAsync(CancellationToken cancellationToken = default)
    {
        var pendingReviewsCount = await _dbSet.CountAsync(
            review => review.Status == ReviewStatus.Pending,
            cancellationToken);
        var pendingCommentsCount = await _context.ReviewComments.CountAsync(
            comment => comment.Status == ReviewStatus.Pending,
            cancellationToken);

        return pendingReviewsCount + pendingCommentsCount;
    }

    public async Task<IReadOnlyList<Review>> GetByToolIdAsync(int toolId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(review => review.ToolId == toolId)
            .ToListAsync(cancellationToken);
    }
}
