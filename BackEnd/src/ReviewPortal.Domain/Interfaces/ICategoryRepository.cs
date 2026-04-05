using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAllWithToolsAsync(CancellationToken cancellationToken = default);
}
