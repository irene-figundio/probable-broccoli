using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using WorkBotAI.Repositories.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class AdminRepository : IAdminRepository
    {
        private readonly WorkBotAIContext _context;

        public AdminRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<AdminStatsDto> GetAdminStatsAsync()
        {
            // === TENANTS ===
            var totalTenants = await _context.Tenants.CountAsync();
            var activeTenants = await _context.Tenants.CountAsync(t => t.IsActive == true);
            var suspendedTenants = totalTenants - activeTenants;

            // === USERS ===
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive == true);

            // === SUBSCRIPTIONS ===
            var activeSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.Status != null && s.Status.Name == "active");
            var trialSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.Status != null && s.Status.Name == "trial");

            // === MRR (Monthly Recurring Revenue) ===
            var subscriptionsWithPlane = await _context.Subscriptions
                .Where(s => s.Status != null && s.Status.Name == "active")
                .Include(s => s.Plane)
                .ToListAsync();

            decimal mrr = 0;
            int basicCount = 0;
            int premiumCount = 0;
            int enterpriseCount = 0;

            foreach (var sub in subscriptionsWithPlane)
            {
                var planName = sub.Plane?.Name?.ToLower() ?? "";
                if (planName.Contains("base") || planName.Contains("basic")) { mrr += 19m; basicCount++; }
                else if (planName.Contains("premium") || planName.Contains("pro")) { mrr += 49m; premiumCount++; }
                else if (planName.Contains("enterprise")) { mrr += 99m; enterpriseCount++; }
                else { mrr += 19m; basicCount++; }
            }
            var arr = mrr * 12;

            // === APPOINTMENTS ===
            var totalAppointments = await _context.Appointments.CountAsync();
            var completedAppointments = await _context.Appointments.CountAsync(a => a.Status != null && a.Status.Name == "completed");
            var cancelledAppointments = await _context.Appointments.CountAsync(a => a.Status != null && a.Status.Name == "cancelled");
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var appointmentsThisMonth = await _context.Appointments.CountAsync(a => a.StartTime >= startOfMonth);
            var startOfWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var appointmentsThisWeek = await _context.Appointments.CountAsync(a => a.StartTime >= startOfWeek);

            return new AdminStatsDto
            {
                Tenants = new TenantStats { Total = totalTenants, Active = activeTenants, Suspended = suspendedTenants, GrowthThisMonth = 4 },
                Users = new UserStats { Total = totalUsers, ActiveToday = activeUsers },
                Revenue = new RevenueStats { Mrr = mrr, Arr = arr, GrowthPercentage = 6.8m },
                Subscriptions = new SubscriptionStats
                {
                    Active = activeSubscriptions,
                    Trial = trialSubscriptions,
                    ByPlan = new PlanDistribution { Basic = basicCount, Pro = premiumCount, Enterprise = enterpriseCount }
                },
                Appointments = new AppointmentStats
                {
                    Total = totalAppointments,
                    ThisWeek = appointmentsThisWeek,
                    ThisMonth = appointmentsThisMonth,
                    Completed = completedAppointments,
                    Cancelled = cancelledAppointments
                },
                Growth = new GrowthStats { Percentage = 25.5m, Trend = "up" }
            };
        }

        public async Task<GlobalSearchResultDto> GlobalSearchAsync(string query, int limit)
        {
            var result = new GlobalSearchResultDto();
            var searchTerm = query.ToLower();

            // Search Tenants
            var tenants = await _context.Tenants
                .Where(t => t.Name.ToLower().Contains(searchTerm) ||
                            (t.Acronym != null && t.Acronym.ToLower().Contains(searchTerm)))
                .Take(limit)
                .Select(t => new SearchResultItemDto
                {
                    Id = t.Id.ToString(),
                    Title = t.Name ?? "Unknown",
                    Subtitle = t.Acronym ?? "",
                    Type = "tenant",
                    AdditionalInfo = t.IsActive == true ? "Attivo" : "Non attivo"
                })
                .ToListAsync();

            // Search Users
            var users = await _context.Users
                .Where(u => u.UserName.ToLower().Contains(searchTerm) ||
                            u.FirstName.ToLower().Contains(searchTerm) ||
                            u.LastName.ToLower().Contains(searchTerm) ||
                            u.Mail.ToLower().Contains(searchTerm))
                .Take(limit)
                .Select(u => new SearchResultItemDto
                {
                    Id = u.Id.ToString(),
                    Title = $"{u.FirstName} {u.LastName}",
                    Subtitle = u.Mail ?? u.UserName ?? "",
                    Type = "user",
                    AdditionalInfo = u.Role != null ? u.Role.Name : "Nessun ruolo"
                })
                .ToListAsync();

            result.Tenants = tenants;
            result.Users = users;

            return result;
        }
    }
}
