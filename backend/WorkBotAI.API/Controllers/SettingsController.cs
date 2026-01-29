using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public SettingsController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Settings/tenant/{tenantId}
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult> GetTenantSettings(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return NotFound(new { success = false, error = "Tenant non trovato" });

        var settings = await _context.Settings
            .Include(s => s.SettingType)
            .Where(s => s.TenantId == tenantId)
            .ToListAsync();

        // Costruisci oggetto impostazioni
        var tenantSettings = new TenantSettingsDto
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

        return Ok(new { success = true, data = tenantSettings });
    }

    // PUT: api/Settings/tenant/{tenantId}
    [HttpPut("tenant/{tenantId}")]
    public async Task<ActionResult> UpdateTenantSettings(Guid tenantId, [FromBody] TenantSettingsDto dto)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return NotFound(new { success = false, error = "Tenant non trovato" });

        // Aggiorna nome tenant se cambiato
        if (!string.IsNullOrEmpty(dto.BusinessName) && dto.BusinessName != tenant.Name)
        {
            tenant.Name = dto.BusinessName;
        }

        // Lista di impostazioni da salvare
        var settingsToSave = new Dictionary<string, string?>
        {
            { "BusinessName", dto.BusinessName },
            { "Phone", dto.Phone },
            { "Email", dto.Email },
            { "Address", dto.Address },
            { "OpeningHours", dto.OpeningHours },
            { "ClosingHours", dto.ClosingHours },
            { "WorkingDays", dto.WorkingDays },
            { "BookingAdvanceDays", dto.BookingAdvanceDays },
            { "CancellationPolicy", dto.CancellationPolicy },
            { "ReminderHours", dto.ReminderHours },
            { "WelcomeMessage", dto.WelcomeMessage },
            { "SmsNotifications", dto.SmsNotifications?.ToString().ToLower() },
            { "EmailNotifications", dto.EmailNotifications?.ToString().ToLower() },
            { "WhatsappNotifications", dto.WhatsappNotifications?.ToString().ToLower() }
        };

        foreach (var (settingName, value) in settingsToSave)
        {
            await SaveSetting(tenantId, settingName, value);
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Impostazioni aggiornate" });
    }

    // GET: api/Settings/types
    [HttpGet("types")]
    public async Task<ActionResult> GetSettingTypes()
    {
        var types = await _context.SettingTypes
            .OrderBy(t => t.Name)
            .Select(t => new SettingTypeDto
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync();

        return Ok(new { success = true, data = types });
    }

    // GET: api/Settings
    [HttpGet]
    public async Task<ActionResult> GetSettings([FromQuery] Guid? tenantId)
    {
        var query = _context.Settings
            .Include(s => s.SettingType)
            .Include(s => s.Tenant)
            .AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(s => s.TenantId == tenantId.Value);

        var settings = await query
            .Select(s => new SettingListDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                SettingTypeId = s.SettingTypeId,
                SettingTypeName = s.SettingType != null ? s.SettingType.Name : null,
                Value = s.Value
            })
            .ToListAsync();

        return Ok(new { success = true, data = settings });
    }

    // POST: api/Settings
    [HttpPost]
    public async Task<ActionResult> CreateSetting([FromBody] CreateSettingDto dto)
    {
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
            return BadRequest(new { success = false, error = "Tenant non trovato" });

        var settingType = await _context.SettingTypes.FindAsync(dto.SettingTypeId);
        if (settingType == null)
            return BadRequest(new { success = false, error = "Tipo impostazione non trovato" });

        // Verifica se esiste già
        var existing = await _context.Settings
            .FirstOrDefaultAsync(s => s.TenantId == dto.TenantId && s.SettingTypeId == dto.SettingTypeId);

        if (existing != null)
            return BadRequest(new { success = false, error = "Impostazione già esistente per questo tenant" });

        var setting = new Setting
        {
            TenantId = dto.TenantId,
            SettingTypeId = dto.SettingTypeId,
            Value = dto.Value
        };

        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSettings), new { tenantId = setting.TenantId },
            new { success = true, data = new { id = setting.Id } });
    }

    // PUT: api/Settings/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateSetting(int id, [FromBody] UpdateSettingDto dto)
    {
        var setting = await _context.Settings.FindAsync(id);
        if (setting == null)
            return NotFound(new { success = false, error = "Impostazione non trovata" });

        setting.Value = dto.Value;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Impostazione aggiornata" });
    }

    // DELETE: api/Settings/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSetting(int id)
    {
        var setting = await _context.Settings.FindAsync(id);
        if (setting == null)
            return NotFound(new { success = false, error = "Impostazione non trovata" });

        _context.Settings.Remove(setting);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Impostazione eliminata" });
    }

    // Helper methods
    private string? GetSettingValue(List<Setting> settings, string typeName)
    {
        return settings.FirstOrDefault(s => s.SettingType?.Name == typeName)?.Value;
    }

    private async Task SaveSetting(Guid tenantId, string typeName, string? value)
    {
        // Trova o crea il tipo
        var settingType = await _context.SettingTypes.FirstOrDefaultAsync(t => t.Name == typeName);
        if (settingType == null)
        {
            settingType = new SettingType { Name = typeName };
            _context.SettingTypes.Add(settingType);
            await _context.SaveChangesAsync();
        }

        // Trova o crea l'impostazione
        var setting = await _context.Settings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.SettingTypeId == settingType.Id);

        if (setting == null)
        {
            setting = new Setting
            {
                TenantId = tenantId,
                SettingTypeId = settingType.Id,
                Value = value
            };
            _context.Settings.Add(setting);
        }
        else
        {
            setting.Value = value;
        }
    }
}