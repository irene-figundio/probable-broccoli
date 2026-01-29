using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TenantDashboardController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public TenantDashboardController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/TenantDashboard/stats/{tenantId}
    [HttpGet("stats/{tenantId}")]
    public async Task<ActionResult> GetStats(Guid tenantId)
    {
        // Verifica che il tenant esista
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant non trovato" });
        }

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Lunedì
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        // Query base per gli appuntamenti del tenant
        var appointmentsQuery = _context.Appointments
            .Where(a => a.TenantId == tenantId && a.IsDeleted != true);

        // Statistiche appuntamenti
        var appointmentsToday = await appointmentsQuery
            .Where(a => a.StartTime >= today && a.StartTime < tomorrow)
            .CountAsync();

        var appointmentsWeek = await appointmentsQuery
            .Where(a => a.StartTime >= startOfWeek && a.StartTime < tomorrow)
            .CountAsync();

        var appointmentsMonth = await appointmentsQuery
            .Where(a => a.StartTime >= startOfMonth && a.StartTime < tomorrow)
            .CountAsync();

        // Statistiche per stato
        var completedAppointments = await appointmentsQuery
            .Where(a => a.Status != null && a.Status.Name == "completed")
            .CountAsync();

        var pendingAppointments = await appointmentsQuery
            .Where(a => a.Status != null && a.Status.Name == "pending")
            .CountAsync();

        var cancelledAppointments = await appointmentsQuery
            .Where(a => a.Status != null && a.Status.Name == "cancelled")
            .CountAsync();

        // Totale clienti del tenant
        var customersTotal = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsDeleted != true)
            .CountAsync();

        var stats = new TenantDashboardStatsDto
        {
            AppointmentsToday = appointmentsToday,
            AppointmentsWeek = appointmentsWeek,
            AppointmentsMonth = appointmentsMonth,
            CustomersTotal = customersTotal,
            CompletedAppointments = completedAppointments,
            PendingAppointments = pendingAppointments,
            CancelledAppointments = cancelledAppointments
        };

        return Ok(new { success = true, data = stats });
    }

    // GET: api/TenantDashboard/appointments/{tenantId}/upcoming
    [HttpGet("appointments/{tenantId}/upcoming")]
    public async Task<ActionResult> GetUpcomingAppointments(Guid tenantId, [FromQuery] int limit = 5)
    {
        var now = DateTime.Now;

        var appointments = await _context.Appointments
            .Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime >= now)
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Status)
            .OrderBy(a => a.StartTime)
            .Take(limit)
            .Select(a => new TenantAppointmentDto
            {
                Id = a.Id,
                CustomerName = a.Customer != null ? a.Customer.FullName : null,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                StatusName = a.Status != null ? a.Status.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note
            })
            .ToListAsync();

        return Ok(new { success = true, data = appointments });
    }

    // GET: api/TenantDashboard/appointments/{tenantId}/today
    [HttpGet("appointments/{tenantId}/today")]
    public async Task<ActionResult> GetTodayAppointments(Guid tenantId)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var appointments = await _context.Appointments
            .Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime >= today && a.StartTime < tomorrow)
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Status)
            .OrderBy(a => a.StartTime)
            .Select(a => new TenantAppointmentDto
            {
                Id = a.Id,
                CustomerName = a.Customer != null ? a.Customer.FullName : null,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                StatusName = a.Status != null ? a.Status.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note
            })
            .ToListAsync();

        return Ok(new { success = true, data = appointments, count = appointments.Count });
    }

    // GET: api/TenantDashboard/appointments/{tenantId}/recent
    [HttpGet("appointments/{tenantId}/recent")]
    public async Task<ActionResult> GetRecentAppointments(Guid tenantId, [FromQuery] int limit = 10)
    {
        var appointments = await _context.Appointments
            .Where(a => a.TenantId == tenantId && a.IsDeleted != true)
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Status)
            .OrderByDescending(a => a.StartTime)
            .Take(limit)
            .Select(a => new TenantAppointmentDto
            {
                Id = a.Id,
                CustomerName = a.Customer != null ? a.Customer.FullName : null,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                StatusName = a.Status != null ? a.Status.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note
            })
            .ToListAsync();

        return Ok(new { success = true, data = appointments });
    }
}