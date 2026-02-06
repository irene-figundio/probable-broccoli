using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class SystemLogRepository : Interfaces.ISystemLogRepository
    {
        private readonly WorkBotAIContext _context;

        public SystemLogRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<SystemLog> Logs, int TotalCount)> GetLogsAsync(
            string? level,
            DateTime? startDate,
            DateTime? endDate,
            Guid? tenantId,
            string? searchTerm,
            int page,
            int pageSize)
        {
            try
            {
                var query = _context.SystemLogs.AsQueryable();

                if (!string.IsNullOrEmpty(level) && level != "all")
                    query = query.Where(l => l.Level.ToLower() == level.ToLower());

                if (startDate.HasValue)
                    query = query.Where(l => l.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(l => l.Timestamp <= endDate.Value.AddDays(1));

                if (tenantId.HasValue)
                    query = query.Where(l => l.TenantId == tenantId.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                    query = query.Where(l =>
                        l.Message.Contains(searchTerm) ||
                        l.Source.Contains(searchTerm) ||
                        (l.Context != null && l.Context.Contains(searchTerm)));

                var totalCount = await query.CountAsync();
                var logs = await query
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (logs, totalCount);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208)
            {
                return (new List<SystemLog>(), 0);
            }
        }

        public async Task<SystemLog?> GetLogByIdAsync(int id)
        {
            return await _context.SystemLogs.FindAsync(id);
        }

        public async Task<SystemLog> CreateLogAsync(SystemLog log)
        {
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<bool> DeleteLogAsync(int id)
        {
            var log = await _context.SystemLogs.FindAsync(id);
            if (log == null) return false;

            _context.SystemLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ClearLogsAsync(DateTime? olderThan)
        {
            var query = _context.SystemLogs.AsQueryable();

            if (olderThan.HasValue)
                query = query.Where(l => l.Timestamp < olderThan.Value);

            return await query.ExecuteDeleteAsync();
        }

        public async Task<object> GetStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);

            try
            {
                // Handle empty table gracefully
                var totalLogs = await _context.SystemLogs.CountAsync();
                if (totalLogs == 0)
                {
                    return GetEmptyStats();
                }

                return new
                {
                    totalLogs,
                    todayLogs = await _context.SystemLogs.CountAsync(l => l.Timestamp >= today),
                    weekLogs = await _context.SystemLogs.CountAsync(l => l.Timestamp >= weekAgo),
                    errorCount = await _context.SystemLogs.CountAsync(l => l.Level == "error"),
                    warningCount = await _context.SystemLogs.CountAsync(l => l.Level == "warning"),
                    infoCount = await _context.SystemLogs.CountAsync(l => l.Level == "info"),
                    byLevel = await _context.SystemLogs
                        .GroupBy(l => l.Level)
                        .Select(g => new { level = g.Key, count = g.Count() })
                        .ToListAsync()
                };
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208)
            {
                return GetEmptyStats();
            }
        }

        private object GetEmptyStats()
        {
            return new
            {
                totalLogs = 0,
                todayLogs = 0,
                weekLogs = 0,
                errorCount = 0,
                warningCount = 0,
                infoCount = 0,
                byLevel = new List<object>()
            };
        }
    }
}
