using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.API.DTOs;
using WorkbotAI.Models;
using WorkBotAI.API.Services;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingRepository _settingRepository;
    private readonly IAuditService _auditService;

    public SettingsController(ISettingRepository settingRepository, IAuditService auditService)
    {
        _settingRepository = settingRepository;
        _auditService = auditService;
    }

    // GET: api/Settings/tenant/{tenantId}
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult> GetTenantSettings(Guid tenantId)
    {
        try
        {
            var tenantSettings = await _settingRepository.GetTenantSettingsAsync(tenantId);
            if (tenantSettings == null)
                return NotFound(new { success = false, error = "Tenant non trovato" });

            return Ok(new { success = true, data = tenantSettings });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Settings", $"Error retrieving settings for tenant {tenantId}", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Settings/tenant/{tenantId}
    [HttpPut("tenant/{tenantId}")]
    public async Task<ActionResult> UpdateTenantSettings(Guid tenantId, [FromBody] TenantSettingsDto dto)
    {
        try
        {
            await _settingRepository.UpdateTenantSettingsAsync(tenantId, dto);
            await _auditService.LogActionAsync("Settings", "UpdateTenantSettings", $"Updated settings for tenant {tenantId}", null, tenantId);
            return Ok(new { success = true, message = "Impostazioni aggiornate" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Settings", $"Error updating settings for tenant {tenantId}", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Settings/types
    [HttpGet("types")]
    public async Task<ActionResult> GetSettingTypes()
    {
        try
        {
            var types = await _settingRepository.GetSettingTypesAsync();
            var typeDtos = types.Select(t => new SettingTypeDto { Id = t.Id, Name = t.Name });
            return Ok(new { success = true, data = typeDtos });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Settings", "Error retrieving setting types", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Settings
    [HttpGet]
    public async Task<ActionResult> GetSettings([FromQuery] Guid? tenantId)
    {
        try
        {
            var settings = await _settingRepository.GetSettingsAsync(tenantId);
            var settingDtos = settings.Select(s => new SettingListDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                SettingTypeId = s.SettingTypeId,
                SettingTypeName = s.SettingType != null ? s.SettingType.Name : null,
                Value = s.Value
            });
            return Ok(new { success = true, data = settingDtos });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Settings", "Error retrieving settings", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/Settings
    [HttpPost]
    public async Task<ActionResult> CreateSetting([FromBody] CreateSettingDto dto)
    {
        try
        {
            var setting = new Setting
            {
                TenantId = dto.TenantId,
                SettingTypeId = dto.SettingTypeId,
                Value = dto.Value
            };
            var createdSetting = await _settingRepository.CreateSettingAsync(setting);
            await _auditService.LogActionAsync("Settings", "Create", $"Created setting {createdSetting.Id} for tenant {dto.TenantId}", null, dto.TenantId);
            return CreatedAtAction(nameof(GetSettings), new { tenantId = createdSetting.TenantId }, new { success = true, data = new { id = createdSetting.Id } });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Settings", "Error creating setting", ex, dto.TenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Settings/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateSetting(int id, [FromBody] UpdateSettingDto dto)
    {
        try
        {
            var setting = new Setting { Id = id, Value = dto.Value };
            await _settingRepository.UpdateSettingAsync(setting);
            await _auditService.LogActionAsync("Settings", "Update", $"Updated setting {id}");
            return Ok(new { success = true, message = "Impostazione aggiornata" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Settings", $"Error updating setting {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/Settings/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSetting(int id)
    {
        try
        {
            await _settingRepository.DeleteSettingAsync(id);
            await _auditService.LogActionAsync("Settings", "Delete", $"Deleted setting {id}");
            return Ok(new { success = true, message = "Impostazione eliminata" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Settings", $"Error deleting setting {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
