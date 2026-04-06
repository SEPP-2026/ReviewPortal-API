using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAllWithToolsAsync(CancellationToken cancellationToken = default);

    Task<Category?> GetByIdWithToolsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default, int? excludedCategoryId = null);
}
