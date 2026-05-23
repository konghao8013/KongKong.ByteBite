using ByteBite.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService) { _dashboardService = dashboardService; }

    [HttpGet("{storeId:guid}/overview")]
    public async Task<object> GetOverview(Guid storeId) => await _dashboardService.GetOverviewAsync(storeId);

    [HttpGet("{storeId:guid}/category-sales")]
    public async Task<object> GetCategorySales(Guid storeId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        => await _dashboardService.GetCategorySalesAsync(storeId, startDate, endDate);

    [HttpGet("{storeId:guid}/hourly")]
    public async Task<object> GetHourlyDistribution(Guid storeId, [FromQuery] DateOnly date)
        => await _dashboardService.GetHourlyDistributionAsync(storeId, date);
}