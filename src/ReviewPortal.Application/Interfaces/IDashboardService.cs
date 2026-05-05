using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Dashboard;

namespace ReviewPortal.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}
