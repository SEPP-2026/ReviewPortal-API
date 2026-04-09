using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Entities;
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
            .Where(tool => tool.IsActive && tool.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tool?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(tool => tool.Category)
            .Include(tool => tool.Images)
            .FirstOrDefaultAsync(tool => tool.Id == id, cancellationToken);
    }
}
