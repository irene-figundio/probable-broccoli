using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface ISystemSettingRepository
    {
        Task<IEnumerable<SystemSetting>> GetAllSettingsAsync();
        Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string category);
        Task<SystemSetting?> GetSettingAsync(string category, string key);
        Task UpdateSettingsAsync(Dictionary<string, Dictionary<string, string?>> settings);
        Task UpdateSettingAsync(string category, string key, string? value);
        Task SeedDefaultSettingsAsync();
        Task EnsureDefaultSettingsAsync();
    }
}
