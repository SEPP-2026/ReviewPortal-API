using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Domain.Interfaces;

public interface IToolRepository : IRepository<Tool>
{
    Task<IReadOnlyList<Tool>> GetActiveByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tool>> GetAllActiveWithDetailsAsync(CancellationToken cancellationToken = default);

    Task<Tool?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
