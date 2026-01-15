using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace WorkBotAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public AnalyticsController(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    // GET: api/Analytics/tenant/{tenantId}
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult> GetTenantAnalytics(Guid tenantId, [FromQuery] string period = "month")
    {
        var analytics = await _analyticsRepository.GetTenantAnalyticsAsync(tenantId, period);
        return Ok(new { success = true, data = analytics });
    }

    // GET: api/Analytics/tenant/{tenantId}/compare
    [HttpGet("tenant/{tenantId}/compare")]
    public async Task<ActionResult> ComparePeriods(Guid tenantId)
    {
        var comparison = await _analyticsRepository.ComparePeriodsAsync(tenantId);
        return Ok(new { success = true, data = comparison });
    }
}