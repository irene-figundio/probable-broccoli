using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface ISystemLogRepository
    {
        Task<(IEnumerable<SystemLog> Logs, int TotalCount)> GetLogsAsync(
            string? level,
            DateTime? startDate,
            DateTime? endDate,
            Guid? tenantId,
            string? searchTerm,
            int page,
            int pageSize);

        Task<SystemLog?> GetLogByIdAsync(int id);
        Task<SystemLog> CreateLogAsync(SystemLog log);
        Task<bool> DeleteLogAsync(int id);
        Task<int> ClearLogsAsync(DateTime? olderThan);
        Task<object> GetStatsAsync();
    }
}
