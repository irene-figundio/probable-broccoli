using WorkBotAI.API.DTOs;
using WorkBotAI.API.Services;
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
    private readonly IAuditService _auditService;

    public AnalyticsController(IAnalyticsRepository analyticsRepository, IAuditService auditService)
    {
        _analyticsRepository = analyticsRepository;
        _auditService = auditService;
    }

    // GET: api/Analytics/tenant/{tenantId}
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult> GetTenantAnalytics(Guid tenantId, [FromQuery] string period = "month")
    {
        try
        {
            var analytics = await _analyticsRepository.GetTenantAnalyticsAsync(tenantId, period);
            return Ok(new { success = true, data = analytics });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Analytics", $"Error retrieving analytics for tenant {tenantId}", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Analytics/tenant/{tenantId}/compare
    [HttpGet("tenant/{tenantId}/compare")]
    public async Task<ActionResult> ComparePeriods(Guid tenantId)
    {
        try
        {
            var comparison = await _analyticsRepository.ComparePeriodsAsync(tenantId);
            return Ok(new { success = true, data = comparison });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Analytics", $"Error comparing periods for tenant {tenantId}", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}