using Microsoft.AspNetCore.Mvc;
using ninjaTax.Models.Services;

namespace ninjaTax.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardApiController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardApiController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var kpis = await _dashboardService.GetKpisAsync();
        return Ok(kpis);
    }

    [HttpGet("cashflow")]
    public async Task<IActionResult> GetCashFlow([FromQuery] int months = 12)
    {
        if (months < 1) months = 12;
        if (months > 36) months = 36;
        var data = await _dashboardService.GetCashFlowAsync(months);
        return Ok(data);
    }

    [HttpGet("topdebtors")]
    public async Task<IActionResult> GetTopDebtors([FromQuery] int count = 10)
    {
        if (count < 1) count = 10;
        if (count > 50) count = 50;
        var data = await _dashboardService.GetTopDebtorsAsync(count);
        return Ok(data);
    }
}
