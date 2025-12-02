using System;
using System.Threading.Tasks;
using WorkBotAI.Repositories.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IAnalyticsRepository
    {
        Task<TenantAnalyticsDto> GetTenantAnalyticsAsync(Guid tenantId, string period);
        Task<PeriodComparisonDto> ComparePeriodsAsync(Guid tenantId);
    }
}
