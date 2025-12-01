using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public AnalyticsController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Analytics/tenant/{tenantId}
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult> GetTenantAnalytics(Guid tenantId, [FromQuery] string period = "month")
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return NotFound(new { success = false, error = "Tenant non trovato" });

        var today = DateTime.Today;
        var startDate = period switch
        {
            "week" => today.AddDays(-7),
            "month" => today.AddMonths(-1),
            "quarter" => today.AddMonths(-3),
            "year" => today.AddYears(-1),
            _ => today.AddMonths(-1)
        };

        // Query base
        var appointmentsQuery = _context.Appointments
            .Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime != null);

        var appointmentsInPeriod = appointmentsQuery
            .Where(a => a.StartTime >= startDate && a.StartTime <= today);

        // Statistiche generali
        var totalAppointments = await appointmentsInPeriod.CountAsync();
        var completedAppointments = await appointmentsInPeriod.Where(a => a.StatusId == 2).CountAsync();
        var cancelledAppointments = await appointmentsInPeriod.Where(a => a.StatusId == 4).CountAsync();
        var pendingAppointments = await appointmentsInPeriod.Where(a => a.StatusId == 1 || a.StatusId == 3).CountAsync();

        // Revenue (da servizi completati)
        var completedWithServices = await appointmentsInPeriod
            .Where(a => a.StatusId == 2)
            .Include(a => a.AppointmentServices)
            .ThenInclude(s => s.Service)
            .ToListAsync();

        var totalRevenue = completedWithServices
            .SelectMany(a => a.AppointmentServices)
            .Sum(s => s.Service?.BasePrice ?? 0);

        // Appuntamenti per giorno della settimana
        var appointmentsList = await appointmentsInPeriod.ToListAsync();
        
        var byDayOfWeek = new Dictionary<string, int>
        {
            { "Lunedì", 0 }, { "Martedì", 0 }, { "Mercoledì", 0 },
            { "Giovedì", 0 }, { "Venerdì", 0 }, { "Sabato", 0 }, { "Domenica", 0 }
        };

        foreach (var appt in appointmentsList.Where(a => a.StartTime.HasValue))
        {
            var dayName = appt.StartTime!.Value.DayOfWeek switch
            {
                DayOfWeek.Monday => "Lunedì",
                DayOfWeek.Tuesday => "Martedì",
                DayOfWeek.Wednesday => "Mercoledì",
                DayOfWeek.Thursday => "Giovedì",
                DayOfWeek.Friday => "Venerdì",
                DayOfWeek.Saturday => "Sabato",
                DayOfWeek.Sunday => "Domenica",
                _ => ""
            };
            if (!string.IsNullOrEmpty(dayName))
                byDayOfWeek[dayName]++;
        }

        // Top servizi
        var topServices = await _context.AppointmentServices
            .Where(s => s.Appointment != null && 
                       s.Appointment.TenantId == tenantId && 
                       s.Appointment.StartTime >= startDate &&
                       s.Appointment.IsDeleted != true)
            .GroupBy(s => new { s.ServiceId, ServiceName = s.Service!.Name ?? "Sconosciuto" })
            .Select(g => new
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.ServiceName,
                Count = g.Count(),
                Revenue = g.Sum(x => x.Service != null ? x.Service.BasePrice ?? 0 : 0)
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        // Top clienti
        var topCustomers = await appointmentsInPeriod
            .Where(a => a.CustomerId != null && a.Customer != null)
            .GroupBy(a => new { a.CustomerId, CustomerName = a.Customer!.FullName ?? "Sconosciuto" })
            .Select(g => new
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                AppointmentsCount = g.Count()
            })
            .OrderByDescending(x => x.AppointmentsCount)
            .Take(5)
            .ToListAsync();

        // Performance staff
        var staffPerformance = await appointmentsInPeriod
            .Where(a => a.StaffId != null && a.Staff != null)
            .GroupBy(a => new { a.StaffId, StaffName = a.Staff!.Name ?? "Sconosciuto" })
            .Select(g => new
            {
                StaffId = g.Key.StaffId,
                StaffName = g.Key.StaffName,
                TotalAppointments = g.Count(),
                CompletedAppointments = g.Count(x => x.StatusId == 2),
                CancelledAppointments = g.Count(x => x.StatusId == 4)
            })
            .OrderByDescending(x => x.TotalAppointments)
            .ToListAsync();

        // Trend appuntamenti (ultimi 7/30 giorni)
        var trendDays = period == "week" ? 7 : 30;
        var trendStartDate = today.AddDays(-trendDays + 1);

        var allAppointmentsForTrend = await appointmentsQuery
            .Where(a => a.StartTime >= trendStartDate && a.StartTime <= today.AddDays(1))
            .ToListAsync();

        var trend = new List<object>();
        for (var date = trendStartDate; date <= today; date = date.AddDays(1))
        {
            var count = allAppointmentsForTrend.Count(a => a.StartTime.HasValue && a.StartTime.Value.Date == date);
            trend.Add(new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Label = date.ToString("dd/MM"),
                Count = count
            });
        }

        // Tasso completamento
        var completionRate = totalAppointments > 0 
            ? Math.Round((double)completedAppointments / totalAppointments * 100, 1) 
            : 0;

        // Tasso cancellazione
        var cancellationRate = totalAppointments > 0 
            ? Math.Round((double)cancelledAppointments / totalAppointments * 100, 1) 
            : 0;

        // Media appuntamenti al giorno
        var daysInPeriod = (today - startDate).Days + 1;
        var avgAppointmentsPerDay = Math.Round((double)totalAppointments / daysInPeriod, 1);

        // Nuovi clienti nel periodo
        var newCustomers = await _context.Customers
            .Where(c => c.TenantId == tenantId && 
                       c.IsDeleted != true && 
                       c.CreationTime >= startDate)
            .CountAsync();

        // Totale clienti
        var totalCustomers = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsDeleted != true)
            .CountAsync();

        var analytics = new
        {
            Period = period,
            StartDate = startDate.ToString("yyyy-MM-dd"),
            EndDate = today.ToString("yyyy-MM-dd"),
            
            Summary = new
            {
                TotalAppointments = totalAppointments,
                CompletedAppointments = completedAppointments,
                CancelledAppointments = cancelledAppointments,
                PendingAppointments = pendingAppointments,
                TotalRevenue = totalRevenue,
                CompletionRate = completionRate,
                CancellationRate = cancellationRate,
                AvgAppointmentsPerDay = avgAppointmentsPerDay,
                TotalCustomers = totalCustomers,
                NewCustomers = newCustomers
            },
            
            AppointmentsByDayOfWeek = byDayOfWeek,
            AppointmentsTrend = trend,
            TopServices = topServices,
            TopCustomers = topCustomers,
            StaffPerformance = staffPerformance
        };

        return Ok(new { success = true, data = analytics });
    }

    // GET: api/Analytics/tenant/{tenantId}/compare
    [HttpGet("tenant/{tenantId}/compare")]
    public async Task<ActionResult> ComparePeriods(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return NotFound(new { success = false, error = "Tenant non trovato" });

        var today = DateTime.Today;
        
        // Questo mese vs mese scorso
        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var lastMonthEnd = thisMonthStart.AddDays(-1);

        var appointmentsQuery = _context.Appointments
            .Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime != null);

        var thisMonthCount = await appointmentsQuery
            .Where(a => a.StartTime >= thisMonthStart && a.StartTime <= today)
            .CountAsync();

        var lastMonthCount = await appointmentsQuery
            .Where(a => a.StartTime >= lastMonthStart && a.StartTime <= lastMonthEnd)
            .CountAsync();

        var appointmentsChange = lastMonthCount > 0 
            ? Math.Round(((double)thisMonthCount - lastMonthCount) / lastMonthCount * 100, 1)
            : (thisMonthCount > 0 ? 100 : 0);

        // Clienti
        var thisMonthNewCustomers = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsDeleted != true && c.CreationTime >= thisMonthStart)
            .CountAsync();

        var lastMonthNewCustomers = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsDeleted != true && 
                       c.CreationTime >= lastMonthStart && c.CreationTime <= lastMonthEnd)
            .CountAsync();

        var customersChange = lastMonthNewCustomers > 0
            ? Math.Round(((double)thisMonthNewCustomers - lastMonthNewCustomers) / lastMonthNewCustomers * 100, 1)
            : (thisMonthNewCustomers > 0 ? 100 : 0);

        var comparison = new
        {
            Appointments = new
            {
                ThisMonth = thisMonthCount,
                LastMonth = lastMonthCount,
                ChangePercent = appointmentsChange,
                Trend = appointmentsChange > 0 ? "up" : (appointmentsChange < 0 ? "down" : "stable")
            },
            NewCustomers = new
            {
                ThisMonth = thisMonthNewCustomers,
                LastMonth = lastMonthNewCustomers,
                ChangePercent = customersChange,
                Trend = customersChange > 0 ? "up" : (customersChange < 0 ? "down" : "stable")
            }
        };

        return Ok(new { success = true, data = comparison });
    }
}