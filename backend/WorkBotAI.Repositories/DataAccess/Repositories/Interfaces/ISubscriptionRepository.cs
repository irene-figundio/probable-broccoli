using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<IEnumerable<Subscription>> GetSubscriptionsAsync(Guid? tenantId, int? statusId);
        Task<Subscription?> GetSubscriptionByIdAsync(int id);
        Task<Subscription> CreateSubscriptionAsync(Subscription subscription);
        Task UpdateSubscriptionAsync(Subscription subscription);
        Task DeleteSubscriptionAsync(int id);
        Task<IEnumerable<Plane>> GetPlanesAsync();
        Task<IEnumerable<SubscriptionStatus>> GetStatusesAsync();
        Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(int days);
        Task<SubscriptionStatus?> GetStatusByIdAsync(int statusId);
        Task<Plane?> GetPlaneByIdAsync(int planeId);
        Task DeactivateActiveSubscriptionsAsync(Guid tenantId);
    }
}
