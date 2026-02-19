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
                await _auditService.LogErrorAsync("Dashboard", $"Tenant {tenantId} not found", null, tenantId);
                return NotFound(new { success = false, error = "Tenant non trovato" });
            }

            var stats = await _dashboardRepository.GetStatsAsync(tenantId);
            if (stats == null)
            {
                await _auditService.LogActionAsync("Dashboard", "GetStats", $"Stats not found for tenant {tenantId}", null, tenantId);
                return Ok(new { success = true, data = new TenantDashboardDto { TenantId = tenantId, TenantName = tenant.Name, Stats = null, UpcomingAppointments = new List<WorkBotAI.API.DTOs.AppointmentListDto>(), RecentAppointments = new List<WorkBotAI.API.DTOs.AppointmentListDto>() } });
            }
            var upcoming = await _dashboardRepository.GetUpcomingAppointmentsAsync(tenantId, 5);
            if (upcoming == null)
                {
                await _auditService.LogActionAsync("Dashboard", "GetUpcomingAppointments", $"Upcoming appointments not found for tenant {tenantId}", null, tenantId);
                return Ok(new { success = true, data = new List<WorkBotAI.API.DTOs.AppointmentListDto>() });
            }
            var recent = await _dashboardRepository.GetRecentAppointmentsAsync(tenantId, 5);
            if (recent == null)
            {
                await _auditService.LogActionAsync("Dashboard", "GetRecentAppointments", $"Recent appointments not found for tenant {tenantId}", null, tenantId);
                return Ok(new { success = true, data = new List<WorkBotAI.API.DTOs.AppointmentListDto>() });
            }

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
