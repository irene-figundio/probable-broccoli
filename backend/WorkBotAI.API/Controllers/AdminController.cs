using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;

namespace WorkBotAI.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public AdminController(WorkBotAIContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        try
        {
            // === TENANTS ===
            var totalTenants = await _context.Tenants.CountAsync();
            var activeTenants = await _context.Tenants
                .Where(t => t.IsActive == true)
                .CountAsync();
            var suspendedTenants = totalTenants - activeTenants;

            // === USERS ===
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users
                .Where(u => u.IsActive == true)
                .CountAsync();

            // === SUBSCRIPTIONS ===
            var activeSubscriptions = await _context.Subscriptions
                .Include(s => s.Status)
                .Where(s => s.Status != null && s.Status.Name == "active")
                .CountAsync();

            var trialSubscriptions = await _context.Subscriptions
                .Include(s => s.Status)
                .Where(s => s.Status != null && s.Status.Name == "trial")
                .CountAsync();

            // === MRR (Monthly Recurring Revenue) ===
            // Prezzi fissi per piano (da configurare nel DB in futuro)
            var subscriptionsWithPlane = await _context.Subscriptions
                .Include(s => s.Status)
                .Include(s => s.Plane)
                .Where(s => s.Status != null && s.Status.Name == "active")
                .ToListAsync();

            decimal mrr = 0;
            int basicCount = 0;
            int premiumCount = 0;
            int enterpriseCount = 0;

            foreach (var sub in subscriptionsWithPlane)
            {
                var planName = sub.Plane?.Name?.ToLower() ?? "";
                
                if (planName.Contains("base") || planName.Contains("basic"))
                {
                    mrr += 19m;
                    basicCount++;
                }
                else if (planName.Contains("premium") || planName.Contains("pro"))
                {
                    mrr += 49m;
                    premiumCount++;
                }
                else if (planName.Contains("enterprise"))
                {
                    mrr += 99m;
                    enterpriseCount++;
                }
                else
                {
                    mrr += 19m; // Default
                    basicCount++;
                }
            }

            var arr = mrr * 12;

            // === APPOINTMENTS ===
            var totalAppointments = await _context.Appointments.CountAsync();
            
            var completedAppointments = await _context.Appointments
                .Include(a => a.Status)
                .Where(a => a.Status != null && a.Status.Name == "completed")
                .CountAsync();

            var cancelledAppointments = await _context.Appointments
                .Include(a => a.Status)
                .Where(a => a.Status != null && a.Status.Name == "cancelled")
                .CountAsync();

            // Appuntamenti questo mese
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var appointmentsThisMonth = await _context.Appointments
                .Where(a => a.StartTime >= startOfMonth)
                .CountAsync();

            // Appuntamenti questa settimana
            var startOfWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var appointmentsThisWeek = await _context.Appointments
                .Where(a => a.StartTime >= startOfWeek)
                .CountAsync();

            // === RESPONSE ===
            return Ok(new
            {
                success = true,
                data = new
                {
                    tenants = new
                    {
                        total = totalTenants,
                        active = activeTenants,
                        suspended = suspendedTenants,
                        growthThisMonth = 4
                    },
                    users = new
                    {
                        total = totalUsers,
                        activeToday = activeUsers
                    },
                    revenue = new
                    {
                        mrr = mrr,
                        arr = arr,
                        growthPercentage = 6.8m
                    },
                    subscriptions = new
                    {
                        active = activeSubscriptions,
                        trial = trialSubscriptions,
                        byPlan = new
                        {
                            basic = basicCount,
                            pro = premiumCount,
                            enterprise = enterpriseCount
                        }
                    },
                    appointments = new
                    {
                        total = totalAppointments,
                        thisWeek = appointmentsThisWeek,
                        thisMonth = appointmentsThisMonth,
                        completed = completedAppointments,
                        cancelled = cancelledAppointments
                    },
                    growth = new
                    {
                        percentage = 25.5m,
                        trend = "up"
                    }
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = $"Errore nel recupero delle statistiche: {ex.Message}"
            });
        }
    }
}