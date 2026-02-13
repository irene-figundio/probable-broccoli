using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Services;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TenantDashboardController : ControllerBase
{
    private readonly ITenantDashboardRepository _dashboardRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IAuditService _auditService;

    public TenantDashboardController(
        ITenantDashboardRepository dashboardRepository,
        ITenantRepository tenantRepository,
        IAuditService auditService)
    {
        _dashboardRepository = dashboardRepository;
        _tenantRepository = tenantRepository;
        _auditService = auditService;
    }

    [HttpGet("{tenantId}")]
    public async Task<ActionResult<TenantDashboardDto>> GetDashboardData(Guid tenantId)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { success = false, error = "Tenant non trovato" });
            }

            var stats = await _dashboardRepository.GetStatsAsync(tenantId);
            var upcoming = await _dashboardRepository.GetUpcomingAppointmentsAsync(tenantId, 5);
            var recent = await _dashboardRepository.GetRecentAppointmentsAsync(tenantId, 5);

            var result = new TenantDashboardDto
            {
                TenantId = tenantId,
                TenantName = tenant.Name,
                Stats = stats,
                UpcomingAppointments = upcoming.Select(a => new WorkBotAI.API.DTOs.AppointmentListDto
                {
                    Id = a.Id,
                    CustomerName = a.Customer?.FullName,
                    StaffName = a.Staff?.Name,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StatusName = a.Status?.Name
                }).ToList(),
                RecentAppointments = recent.Select(a => new WorkBotAI.API.DTOs.AppointmentListDto
                {
                    Id = a.Id,
                    CustomerName = a.Customer?.FullName,
                    StaffName = a.Staff?.Name,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    StatusName = a.Status?.Name
                }).ToList()
            };

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Dashboard", $"Error retrieving dashboard for {tenantId}", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
