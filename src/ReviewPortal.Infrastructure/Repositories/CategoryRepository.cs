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

    public async Task<Category?> GetByIdWithToolsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Tools)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default, int? excludedCategoryId = null)
    {
        return await _dbSet.AnyAsync(
            category => category.Name == name && (!excludedCategoryId.HasValue || category.Id != excludedCategoryId.Value),
            cancellationToken);
    }
}
