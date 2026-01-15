using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemSettingsController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public SystemSettingsController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/SystemSettings
    [HttpGet]
    public async Task<ActionResult> GetAllSettings()
    {
        var settings = await _context.SystemSettings
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync();

        // Raggruppa per categoria
        var grouped = settings
            .GroupBy(s => s.Category)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(s => s.Key, s => s.Value)
            );

        return Ok(new { success = true, data = grouped });
    }

    // GET: api/SystemSettings/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult> GetByCategory(string category)
    {
        var settings = await _context.SystemSettings
            .Where(s => s.Category == category)
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        return Ok(new { success = true, data = settings });
    }

    // GET: api/SystemSettings/{category}/{key}
    [HttpGet("{category}/{key}")]
    public async Task<ActionResult> GetSetting(string category, string key)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Category == category && s.Key == key);

        if (setting == null)
            return NotFound(new { success = false, error = "Impostazione non trovata" });

        return Ok(new { success = true, data = setting.Value });
    }

    // PUT: api/SystemSettings
    [HttpPut]
    public async Task<ActionResult> UpdateSettings([FromBody] Dictionary<string, Dictionary<string, string?>> settings)
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
        return Ok(new { success = true, message = "Impostazioni aggiornate" });
    }

    // PUT: api/SystemSettings/{category}/{key}
    [HttpPut("{category}/{key}")]
    public async Task<ActionResult> UpdateSetting(string category, string key, [FromBody] string? value)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Category == category && s.Key == key);

        if (setting == null)
        {
            setting = new SystemSetting
            {
                Category = category,
                Key = key,
                Value = value,
                UpdatedAt = DateTime.UtcNow
            };
            _context.SystemSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new { success = true, message = "Impostazione aggiornata" });
    }

    // POST: api/SystemSettings/seed
    [HttpPost("seed")]
    public async Task<ActionResult> SeedSettings()
    {
        // Verifica se esistono già impostazioni
        if (await _context.SystemSettings.AnyAsync())
            return BadRequest(new { success = false, error = "Impostazioni già presenti" });

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

        return Ok(new { success = true, message = $"{defaultSettings.Count} impostazioni create" });
    }
}