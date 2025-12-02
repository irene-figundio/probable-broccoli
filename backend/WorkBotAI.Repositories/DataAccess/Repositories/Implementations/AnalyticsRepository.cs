using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using WorkBotAI.Repositories.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly WorkBotAIContext _context;

        public AnalyticsRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<TenantAnalyticsDto> GetTenantAnalyticsAsync(Guid tenantId, string period)
        {
            var today = DateTime.Today;
            var startDate = period switch
            {
                "week" => today.AddDays(-7),
                "month" => today.AddMonths(-1),
                "quarter" => today.AddMonths(-3),
                "year" => today.AddYears(-1),
                _ => today.AddMonths(-1)
            };

            var appointmentsQuery = _context.Appointments.Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime != null);
            var appointmentsInPeriod = appointmentsQuery.Where(a => a.StartTime >= startDate && a.StartTime <= today);

            var totalAppointments = await appointmentsInPeriod.CountAsync();
            var completedAppointments = await appointmentsInPeriod.CountAsync(a => a.StatusId == 2);
            var cancelledAppointments = await appointmentsInPeriod.CountAsync(a => a.StatusId == 4);
            var pendingAppointments = await appointmentsInPeriod.CountAsync(a => a.StatusId == 1 || a.StatusId == 3);

            var completedWithServices = await appointmentsInPeriod.Where(a => a.StatusId == 2).Include(a => a.AppointmentServices).ThenInclude(s => s.Service).ToListAsync();
            var totalRevenue = completedWithServices.SelectMany(a => a.AppointmentServices).Sum(s => s.Service?.BasePrice ?? 0);

            var appointmentsList = await appointmentsInPeriod.ToListAsync();
            var byDayOfWeek = new Dictionary<string, int> { { "Lunedì", 0 }, { "Martedì", 0 }, { "Mercoledì", 0 }, { "Giovedì", 0 }, { "Venerdì", 0 }, { "Sabato", 0 }, { "Domenica", 0 } };
            foreach (var appt in appointmentsList.Where(a => a.StartTime.HasValue))
            {
                var dayName = appt.StartTime!.Value.DayOfWeek switch { DayOfWeek.Monday => "Lunedì", DayOfWeek.Tuesday => "Martedì", DayOfWeek.Wednesday => "Mercoledì", DayOfWeek.Thursday => "Giovedì", DayOfWeek.Friday => "Venerdì", DayOfWeek.Saturday => "Sabato", DayOfWeek.Sunday => "Domenica", _ => "" };
                if (!string.IsNullOrEmpty(dayName)) byDayOfWeek[dayName]++;
            }

            var topServices = await _context.AppointmentServices.Where(s => s.Appointment != null && s.Appointment.TenantId == tenantId && s.Appointment.StartTime >= startDate && s.Appointment.IsDeleted != true).GroupBy(s => new { s.ServiceId, ServiceName = s.Service!.Name ?? "Sconosciuto" }).Select(g => new { ServiceId = g.Key.ServiceId, ServiceName = g.Key.ServiceName, Count = g.Count(), Revenue = g.Sum(x => x.Service != null ? x.Service.BasePrice ?? 0 : 0) }).OrderByDescending(x => x.Count).Take(5).ToListAsync<object>();
            var topCustomers = await appointmentsInPeriod.Where(a => a.CustomerId != null && a.Customer != null).GroupBy(a => new { a.CustomerId, CustomerName = a.Customer!.FullName ?? "Sconosciuto" }).Select(g => new { CustomerId = g.Key.CustomerId, CustomerName = g.Key.CustomerName, AppointmentsCount = g.Count() }).OrderByDescending(x => x.AppointmentsCount).Take(5).ToListAsync<object>();
            var staffPerformance = await appointmentsInPeriod.Where(a => a.StaffId != null && a.Staff != null).GroupBy(a => new { a.StaffId, StaffName = a.Staff!.Name ?? "Sconosciuto" }).Select(g => new { StaffId = g.Key.StaffId, StaffName = g.Key.StaffName, TotalAppointments = g.Count(), CompletedAppointments = g.Count(x => x.StatusId == 2), CancelledAppointments = g.Count(x => x.StatusId == 4) }).OrderByDescending(x => x.TotalAppointments).ToListAsync<object>();

            var trendDays = period == "week" ? 7 : 30;
            var trendStartDate = today.AddDays(-trendDays + 1);
            var allAppointmentsForTrend = await appointmentsQuery.Where(a => a.StartTime >= trendStartDate && a.StartTime <= today.AddDays(1)).ToListAsync();
            var trend = new List<object>();
            for (var date = trendStartDate; date <= today; date = date.AddDays(1))
            {
                var count = allAppointmentsForTrend.Count(a => a.StartTime.HasValue && a.StartTime.Value.Date == date);
                trend.Add(new { Date = date.ToString("yyyy-MM-dd"), Label = date.ToString("dd/MM"), Count = count });
            }

            var completionRate = totalAppointments > 0 ? Math.Round((double)completedAppointments / totalAppointments * 100, 1) : 0;
            var cancellationRate = totalAppointments > 0 ? Math.Round((double)cancelledAppointments / totalAppointments * 100, 1) : 0;
            var daysInPeriod = (today - startDate).Days + 1;
            var avgAppointmentsPerDay = Math.Round((double)totalAppointments / daysInPeriod, 1);
            var newCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId && c.IsDeleted != true && c.CreationTime >= startDate);
            var totalCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId && c.IsDeleted != true);

            return new TenantAnalyticsDto
            {
                Period = period,
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = today.ToString("yyyy-MM-dd"),
                Summary = new AnalyticsSummary { TotalAppointments = totalAppointments, CompletedAppointments = completedAppointments, CancelledAppointments = cancelledAppointments, PendingAppointments = pendingAppointments, TotalRevenue = totalRevenue, CompletionRate = completionRate, CancellationRate = cancellationRate, AvgAppointmentsPerDay = avgAppointmentsPerDay, TotalCustomers = totalCustomers, NewCustomers = newCustomers },
                AppointmentsByDayOfWeek = byDayOfWeek,
                AppointmentsTrend = trend,
                TopServices = topServices,
                TopCustomers = topCustomers,
                StaffPerformance = staffPerformance
            };
        }

        public async Task<PeriodComparisonDto> ComparePeriodsAsync(Guid tenantId)
        {
            var today = DateTime.Today;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            var appointmentsQuery = _context.Appointments.Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime != null);

            var thisMonthCount = await appointmentsQuery.CountAsync(a => a.StartTime >= thisMonthStart && a.StartTime <= today);
            var lastMonthCount = await appointmentsQuery.CountAsync(a => a.StartTime >= lastMonthStart && a.StartTime <= lastMonthEnd);
            var appointmentsChange = lastMonthCount > 0 ? Math.Round(((double)thisMonthCount - lastMonthCount) / lastMonthCount * 100, 1) : (thisMonthCount > 0 ? 100 : 0);

            var thisMonthNewCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId && c.IsDeleted != true && c.CreationTime >= thisMonthStart);
            var lastMonthNewCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId && c.IsDeleted != true && c.CreationTime >= lastMonthStart && c.CreationTime <= lastMonthEnd);
            var customersChange = lastMonthNewCustomers > 0 ? Math.Round(((double)thisMonthNewCustomers - lastMonthNewCustomers) / lastMonthNewCustomers * 100, 1) : (thisMonthNewCustomers > 0 ? 100 : 0);

            return new PeriodComparisonDto
            {
                Appointments = new ComparisonItem { ThisMonth = thisMonthCount, LastMonth = lastMonthCount, ChangePercent = appointmentsChange, Trend = appointmentsChange > 0 ? "up" : (appointmentsChange < 0 ? "down" : "stable") },
                NewCustomers = new ComparisonItem { ThisMonth = thisMonthNewCustomers, LastMonth = lastMonthNewCustomers, ChangePercent = customersChange, Trend = customersChange > 0 ? "up" : (customersChange < 0 ? "down" : "stable") }
            };
        }
    }
}
