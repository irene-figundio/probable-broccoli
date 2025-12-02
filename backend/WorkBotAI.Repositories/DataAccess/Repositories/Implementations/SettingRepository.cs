using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class SettingRepository : ISettingRepository
    {
        private readonly WorkBotAIContext _context;

        public SettingRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<TenantSettingsDto> GetTenantSettingsAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return null;

            var settings = await _context.Settings.Include(s => s.SettingType).Where(s => s.TenantId == tenantId).ToListAsync();
            return new TenantSettingsDto
            {
                TenantId = tenantId,
                TenantName = tenant.Name,
                BusinessName = GetSettingValue(settings, "BusinessName") ?? tenant.Name,
                Phone = GetSettingValue(settings, "Phone"),
                Email = GetSettingValue(settings, "Email"),
                Address = GetSettingValue(settings, "Address"),
                OpeningHours = GetSettingValue(settings, "OpeningHours") ?? "09:00",
                ClosingHours = GetSettingValue(settings, "ClosingHours") ?? "18:00",
                WorkingDays = GetSettingValue(settings, "WorkingDays") ?? "1,2,3,4,5,6",
                BookingAdvanceDays = GetSettingValue(settings, "BookingAdvanceDays") ?? "30",
                CancellationPolicy = GetSettingValue(settings, "CancellationPolicy") ?? "24",
                ReminderHours = GetSettingValue(settings, "ReminderHours") ?? "24",
                WelcomeMessage = GetSettingValue(settings, "WelcomeMessage"),
                SmsNotifications = GetSettingValue(settings, "SmsNotifications") == "true",
                EmailNotifications = GetSettingValue(settings, "EmailNotifications") == "true",
                WhatsappNotifications = GetSettingValue(settings, "WhatsappNotifications") == "true"
            };
        }

        public async Task UpdateTenantSettingsAsync(Guid tenantId, TenantSettingsDto dto)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant != null && !string.IsNullOrEmpty(dto.BusinessName) && dto.BusinessName != tenant.Name)
            {
                tenant.Name = dto.BusinessName;
            }

            var settingsToSave = new Dictionary<string, string?>
            {
                { "BusinessName", dto.BusinessName }, { "Phone", dto.Phone }, { "Email", dto.Email }, { "Address", dto.Address },
                { "OpeningHours", dto.OpeningHours }, { "ClosingHours", dto.ClosingHours }, { "WorkingDays", dto.WorkingDays },
                { "BookingAdvanceDays", dto.BookingAdvanceDays }, { "CancellationPolicy", dto.CancellationPolicy }, { "ReminderHours", dto.ReminderHours },
                { "WelcomeMessage", dto.WelcomeMessage }, { "SmsNotifications", dto.SmsNotifications?.ToString().ToLower() },
                { "EmailNotifications", dto.EmailNotifications?.ToString().ToLower() }, { "WhatsappNotifications", dto.WhatsappNotifications?.ToString().ToLower() }
            };

            foreach (var (settingName, value) in settingsToSave)
            {
                await SaveSetting(tenantId, settingName, value);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SettingType>> GetSettingTypesAsync()
        {
            return await _context.SettingTypes.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<IEnumerable<Setting>> GetSettingsAsync(Guid? tenantId)
        {
            var query = _context.Settings.Include(s => s.SettingType).Include(s => s.Tenant).AsQueryable();
            if (tenantId.HasValue) query = query.Where(s => s.TenantId == tenantId.Value);
            return await query.ToListAsync();
        }

        public async Task<Setting> CreateSettingAsync(Setting setting)
        {
            _context.Settings.Add(setting);
            await _context.SaveChangesAsync();
            return setting;
        }

        public async Task UpdateSettingAsync(Setting setting)
        {
            _context.Entry(setting).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSettingAsync(int id)
        {
            var setting = await _context.Settings.FindAsync(id);
            if (setting != null)
            {
                _context.Settings.Remove(setting);
                await _context.SaveChangesAsync();
            }
        }

        private string GetSettingValue(List<Setting> settings, string typeName) => settings.FirstOrDefault(s => s.SettingType?.Name == typeName)?.Value;

        private async Task SaveSetting(Guid tenantId, string typeName, string value)
        {
            var settingType = await _context.SettingTypes.FirstOrDefaultAsync(t => t.Name == typeName);
            if (settingType == null)
            {
                settingType = new SettingType { Name = typeName };
                _context.SettingTypes.Add(settingType);
                await _context.SaveChangesAsync();
            }

            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SettingTypeId == settingType.Id);
            if (setting == null)
            {
                setting = new Setting { TenantId = tenantId, SettingTypeId = settingType.Id, Value = value };
                _context.Settings.Add(setting);
            }
            else
            {
                setting.Value = value;
            }
        }
    }
}
