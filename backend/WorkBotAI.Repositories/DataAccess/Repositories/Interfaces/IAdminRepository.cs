using System.Collections.Generic;
using System.Threading.Tasks;
using WorkBotAI.Repositories.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<AdminStatsDto> GetAdminStatsAsync();
        Task<AdminTenantStatsDto> GetAdminStatsByIdAsync(Guid id);
        Task<GlobalSearchResultDto> GlobalSearchAsync(string query, int limit);
    }
}
