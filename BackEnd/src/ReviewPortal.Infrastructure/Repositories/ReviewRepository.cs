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

    public async Task<IReadOnlyList<Review>> GetApprovedByToolIdWithDetailsAsync(int toolId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(review => review.Comments)
            .Include(review => review.CompanyResponse)
                .ThenInclude(response => response!.StaffUser)
            .Where(review => review.ToolId == toolId && review.Status == ReviewStatus.Approved)
            .OrderByDescending(review => review.CreatedDate)
            .ThenByDescending(review => review.Id)
            .ToListAsync(cancellationToken);
    }
}
