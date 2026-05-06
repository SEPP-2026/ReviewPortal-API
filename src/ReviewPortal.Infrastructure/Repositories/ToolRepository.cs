using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Domain.Interfaces;
using ReviewPortal.Infrastructure.Data;

namespace ReviewPortal.Infrastructure.Repositories;

public class ToolRepository : Repository<Tool>, IToolRepository
{
    public ToolRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Tool>> GetActiveByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(tool => tool.Category)
            .Include(tool => tool.Images)
            .Include(tool => tool.Reviews.Where(review => review.Status == ReviewStatus.Approved))
            .Where(tool => tool.IsActive && tool.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tool>> GetAllActiveWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(tool => tool.Category)
            .Include(tool => tool.Images)
            .Include(tool => tool.Reviews.Where(review => review.Status == ReviewStatus.Approved))
            .Where(tool => tool.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tool>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(tool => tool.Category)
            .Include(tool => tool.Images)
            .Include(tool => tool.Reviews.Where(review => review.Status == ReviewStatus.Approved))
            .ToListAsync(cancellationToken);
    }

    public async Task<Tool?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(tool => tool.Category)
            .Include(tool => tool.Images)
            .Include(tool => tool.Reviews.Where(review => review.Status == ReviewStatus.Approved))
            .FirstOrDefaultAsync(tool => tool.Id == id, cancellationToken);
    }
}
