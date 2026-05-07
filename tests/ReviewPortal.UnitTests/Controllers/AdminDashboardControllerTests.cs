using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewPortal.API.Controllers.Admin;
using ReviewPortal.Application.Common;
using ReviewPortal.Application.DTOs.Dashboard;
using ReviewPortal.Application.Interfaces;

namespace ReviewPortal.UnitTests.Controllers;

public class AdminDashboardControllerTests
{
    [Fact]
    public void Controller_HasAuthorizeAttributeForAdminRole()
    {
        var attribute = typeof(AdminDashboardController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute!.Roles);
    }

    [Fact]
    public void Controller_WhenNoTokenIsProvided_IsProtectedByAuthorizeAttribute()
    {
        var attribute = typeof(AdminDashboardController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
    }

    [Fact]
    public void Controller_WhenCustomerRoleIsUsed_IsExcludedFromAllowedRoles()
    {
        var attribute = typeof(AdminDashboardController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.DoesNotContain("Customer", attribute!.Roles!.Split(','));
    }

    [Fact]
    public void Controller_UsesAdminDashboardRoute()
    {
        var attribute = typeof(AdminDashboardController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal("api/admin/dashboard", attribute!.Template);
    }

    [Fact]
    public async Task GetStats_WhenStatsAreAvailable_ReturnsOk()
    {
        var stats = CreateDashboardStats();
        var dashboardService = new FakeDashboardService
        {
            StatsResult = Result<DashboardStatsDto>.Success(stats)
        };
        var controller = new AdminDashboardController(dashboardService);

        var result = await controller.GetStats(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(stats, okResult.Value);
        Assert.Equal(1, dashboardService.GetStatsCallCount);
    }

    private static DashboardStatsDto CreateDashboardStats()
    {
        return new DashboardStatsDto(
            TotalActiveTools: 8,
            TotalInactiveTools: 2,
            PendingModerationCount: 3,
            ReviewsPublishedThisMonth: 6,
            TopRatedTools:
            [
                new ToolRankingDto(1, "Tower Scaffold", 4.8m, 7)
            ],
            MostReviewedTools:
            [
                new ToolRankingDto(2, "Pressure Washer", 4.5m, 12)
            ]);
    }

    private sealed class FakeDashboardService : IDashboardService
    {
        public int GetStatsCallCount { get; private set; }

        public Result<DashboardStatsDto> StatsResult { get; set; } =
            Result<DashboardStatsDto>.Success(CreateDashboardStats());

        public Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
        {
            GetStatsCallCount++;
            return Task.FromResult(StatsResult);
        }
    }
}
