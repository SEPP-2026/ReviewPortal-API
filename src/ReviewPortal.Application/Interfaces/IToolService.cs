using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Tools;

namespace ReviewPortal.Application.Interfaces;

public interface IToolService
{
    Task<Result<PagedList<ToolSummaryDto>>> GetToolsByCategoryAsync(int categoryId, int page, int pageSize, string? sortBy = null, CancellationToken cancellationToken = default);

    Task<Result<ToolDto>> GetToolByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PagedList<AdminToolSummaryDto>>> GetAdminToolsAsync(AdminToolQueryRequest request, CancellationToken cancellationToken = default);

    Task<Result<ToolDto>> GetAdminToolByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PagedList<ToolSummaryDto>>> SearchToolsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Result<PagedList<ToolSummaryDto>>> FilterByPriceRangeAsync(int categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize, string? sortBy = null, CancellationToken cancellationToken = default);

    Task<Result<RentalCalculationResponse>> CalculateRentalCostAsync(int toolId, RentalCalculationRequest request, CancellationToken cancellationToken = default);

    Task<Result<ToolDto>> CreateToolAsync(CreateToolRequest request, CancellationToken cancellationToken = default);

    Task<Result<ToolDto>> UpdateToolAsync(int id, UpdateToolRequest request, CancellationToken cancellationToken = default);

    Task<Result<bool>> SetToolStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
