using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class SystemSettingRepository : Interfaces.ISystemSettingRepository
    {
        private readonly WorkBotAIContext _context;

        public SystemSettingRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SystemSetting>> GetAllSettingsAsync()
        {
            await EnsureDefaultSettingsAsync();
            return await _context.SystemSettings
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Key)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemSetting>> GetByCategoryAsync(string category)
        {
            await EnsureDefaultSettingsAsync();
            return await _context.SystemSettings
                .Where(s => s.Category == category)
                .ToListAsync();
        }

        public async Task<SystemSetting?> GetSettingAsync(string category, string key)
        {
            await EnsureDefaultSettingsAsync();
            return await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Category == category && s.Key == key);
        }

        public async Task UpdateSettingsAsync(Dictionary<string, Dictionary<string, string?>> settings)
        {
            foreach (var category in settings)
            {
                foreach (var kvp in category.Value)
                {
                    var existing = await _context.SystemSettings
                        .FirstOrDefaultAsync(s => s.Category == category.Key && s.Key == kvp.Key);

                    if (existing != null)
                    {
                        existing.Value = kvp.Value;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.SystemSettings.Add(new SystemSetting
                        {
                            Category = category.Key,
                            Key = kvp.Key,
                            Value = kvp.Value,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSettingAsync(string category, string key, string? value)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Category == category && s.Key == key);

            if (setting == null)
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    Category = category,
                    Key = key,
                    Value = value,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task EnsureDefaultSettingsAsync()
        {
            try
            {
                if (!await _context.SystemSettings.AnyAsync())
                {
                    await SeedDefaultSettingsAsync();
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208) // Invalid object name
            {
                // La tabella non esiste ancora. In un ambiente reale dovremmo eseguire le migrazioni
                // o informare l'utente. Per ora evitiamo il crash.
                System.Diagnostics.Debug.WriteLine("La tabella SystemSettings non esiste nel database.");
            }
            catch (Exception)
            {
                // Altri errori
            }
        }

        public async Task SeedDefaultSettingsAsync()
        {
            var defaultSettings = new List<SystemSetting>
            {
                // General
                new() { Category = "general", Key = "platformName", Value = "WorkBotAI", Description = "Nome della piattaforma" },
                new() { Category = "general", Key = "supportEmail", Value = "support@workbotai.com", Description = "Email supporto" },
                new() { Category = "general", Key = "defaultLanguage", Value = "it", Description = "Lingua predefinita" },
                new() { Category = "general", Key = "timezone", Value = "Europe/Rome", Description = "Fuso orario" },
                new() { Category = "general", Key = "dateFormat", Value = "DD/MM/YYYY", Description = "Formato data" },
                new() { Category = "general", Key = "maintenanceMode", Value = "false", Description = "Modalità manutenzione" },

                // Email
                new() { Category = "email", Key = "smtpHost", Value = "smtp.gmail.com", Description = "Host SMTP" },
                new() { Category = "email", Key = "smtpPort", Value = "587", Description = "Porta SMTP" },
                new() { Category = "email", Key = "smtpUser", Value = "", Description = "Username SMTP" },
                new() { Category = "email", Key = "smtpPassword", Value = "", Description = "Password SMTP" },
                new() { Category = "email", Key = "smtpSecure", Value = "tls", Description = "Sicurezza SMTP" },
                new() { Category = "email", Key = "fromEmail", Value = "noreply@workbotai.com", Description = "Email mittente" },
                new() { Category = "email", Key = "fromName", Value = "WorkBotAI", Description = "Nome mittente" },

                // Security
                new() { Category = "security", Key = "sessionTimeout", Value = "60", Description = "Timeout sessione (minuti)" },
                new() { Category = "security", Key = "maxLoginAttempts", Value = "5", Description = "Tentativi login massimi" },
                new() { Category = "security", Key = "passwordMinLength", Value = "8", Description = "Lunghezza minima password" },
                new() { Category = "security", Key = "requireSpecialChar", Value = "true", Description = "Richiedi caratteri speciali" },
                new() { Category = "security", Key = "twoFactorEnabled", Value = "false", Description = "2FA abilitato" },
                new() { Category = "security", Key = "ipWhitelist", Value = "", Description = "IP whitelist" },

                // Notifications
                new() { Category = "notifications", Key = "emailNotifications", Value = "true", Description = "Notifiche email" },
                new() { Category = "notifications", Key = "smsNotifications", Value = "false", Description = "Notifiche SMS" },
                new() { Category = "notifications", Key = "pushNotifications", Value = "true", Description = "Notifiche push" },
                new() { Category = "notifications", Key = "adminAlerts", Value = "true", Description = "Alert admin" },
                new() { Category = "notifications", Key = "weeklyReports", Value = "true", Description = "Report settimanali" },

                // Appearance
                new() { Category = "appearance", Key = "primaryColor", Value = "#3B82F6", Description = "Colore primario" },
                new() { Category = "appearance", Key = "darkMode", Value = "auto", Description = "Modalità scura" },
                new() { Category = "appearance", Key = "logoUrl", Value = "", Description = "URL logo" },
                new() { Category = "appearance", Key = "faviconUrl", Value = "", Description = "URL favicon" }
            };

            _context.SystemSettings.AddRange(defaultSettings);
            await _context.SaveChangesAsync();
        }
    }
}
