using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class SubscriptionRepository : Interfaces.ISubscriptionRepository
    {
        private readonly WorkBotAIContext _context;

        public SubscriptionRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsAsync(Guid? tenantId, int? statusId)
        {
            var query = _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plane)
                .Include(s => s.Status)
                .AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(s => s.TenantId == tenantId.Value);

            if (statusId.HasValue)
                query = query.Where(s => s.StatusId == statusId.Value);

            return await query.OrderByDescending(s => s.StartDate).ToListAsync();
        }

        public async Task<Subscription?> GetSubscriptionByIdAsync(int id)
        {
            return await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plane)
                .Include(s => s.Status)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task UpdateSubscriptionAsync(Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSubscriptionAsync(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Plane>> GetPlanesAsync()
        {
            return await _context.Planes.Where(p => p.IsActive == true).ToListAsync();
        }

        public async Task<IEnumerable<SubscriptionStatus>> GetStatusesAsync()
        {
            return await _context.SubscriptionStatuses.Where(s => s.IsActive == true).ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(int days)
        {
            var limitDate = DateOnly.FromDateTime(DateTime.Today.AddDays(days));
            return await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plane)
                .Where(s => s.Status != null && s.Status.Name == "active" && s.EndDate <= limitDate)
                .OrderBy(s => s.EndDate)
                .ToListAsync();
        }

        public async Task<SubscriptionStatus?> GetStatusByIdAsync(int statusId)
        {
            return await _context.SubscriptionStatuses.FindAsync(statusId);
        }

        public async Task<Plane?> GetPlaneByIdAsync(int planeId)
        {
            return await _context.Planes.FindAsync(planeId);
        }

        public async Task DeactivateActiveSubscriptionsAsync(Guid tenantId)
        {
            var activeSubscriptions = await _context.Subscriptions
                .Where(s => s.TenantId == tenantId && s.Status != null && s.Status.Name == "active")
                .ToListAsync();

            foreach (var sub in activeSubscriptions)
            {
                sub.StatusId = 3; // expired
            }
            await _context.SaveChangesAsync();
        }
    }
}
