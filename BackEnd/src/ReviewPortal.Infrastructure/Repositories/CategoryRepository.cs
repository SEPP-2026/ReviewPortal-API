using Microsoft.EntityFrameworkCore;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Interfaces;
using ReviewPortal.Infrastructure.Data;

namespace ReviewPortal.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Category>> GetAllWithToolsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Tools)
            .ToListAsync(cancellationToken);
    }
}
