using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface ISettingRepository
    {
        Task<TenantSettingsDto> GetTenantSettingsAsync(Guid tenantId);
        Task UpdateTenantSettingsAsync(Guid tenantId, TenantSettingsDto dto);
        Task<IEnumerable<SettingType>> GetSettingTypesAsync();
        Task<IEnumerable<Setting>> GetSettingsAsync(Guid? tenantId);
        Task<Setting> GetSettingByIdAsync(int id);
        Task<Setting> CreateSettingAsync(Setting setting);
        Task UpdateSettingAsync(Setting setting);
        Task DeleteSettingAsync(int id);
    }
}
