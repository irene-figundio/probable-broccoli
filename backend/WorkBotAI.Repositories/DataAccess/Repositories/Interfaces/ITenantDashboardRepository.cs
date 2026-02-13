using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface ITenantDashboardRepository
    {
        Task<object> GetStatsAsync(Guid tenantId);
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(Guid tenantId, int count);
        Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(Guid tenantId, int count);
    }
}
