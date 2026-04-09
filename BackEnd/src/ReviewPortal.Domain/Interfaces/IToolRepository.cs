using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Domain.Interfaces;

public interface IToolRepository : IRepository<Tool>
{
    Task<IReadOnlyList<Tool>> GetActiveByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
}
