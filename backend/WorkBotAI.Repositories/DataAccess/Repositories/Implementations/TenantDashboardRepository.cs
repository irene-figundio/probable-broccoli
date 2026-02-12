using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class TenantDashboardRepository : ITenantDashboardRepository
    {
        private readonly WorkBotAIContext _context;

        public TenantDashboardRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<object> GetStatsAsync(Guid tenantId)
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var appointmentsTotal = await _context.Appointments
                .CountAsync(a => a.TenantId == tenantId && a.IsDeleted != true);

            var appointmentsMonth = await _context.Appointments
                .CountAsync(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime >= startOfMonth);

            var customersTotal = await _context.Customers
                .CountAsync(c => c.TenantId == tenantId && c.IsDeleted != true);

            var revenue = await _context.Appointments
                .Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StatusId == 3) // Completed
                .SelectMany(a => a.AppointmentServices)
                .SumAsync(s => s.Service != null ? s.Service.BasePrice : 0);

            return new
            {
                appointmentsTotal,
                appointmentsMonth,
                customersTotal,
                revenue
            };
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(Guid tenantId, int count)
        {
            return await _context.Appointments
                .Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime >= DateTime.Now)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Include(a => a.Status)
                .OrderBy(a => a.StartTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(Guid tenantId, int count)
        {
            return await _context.Appointments
                .Where(a => a.TenantId == tenantId && a.IsDeleted != true && a.StartTime < DateTime.Now)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Include(a => a.Status)
                .OrderByDescending(a => a.StartTime)
                .Take(count)
                .ToListAsync();
        }
    }
}
