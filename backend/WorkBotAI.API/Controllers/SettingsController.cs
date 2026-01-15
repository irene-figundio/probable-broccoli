using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.API.DTOs;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using System.Linq;

namespace WorkBotAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingRepository _settingRepository;

    public SettingsController(ISettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    // GET: api/Settings/tenant/{tenantId}
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult> GetTenantSettings(Guid tenantId)
    {
        var tenantSettings = await _settingRepository.GetTenantSettingsAsync(tenantId);
        if (tenantSettings == null)
            return NotFound(new { success = false, error = "Tenant non trovato" });

        return Ok(new { success = true, data = tenantSettings });
    }

    // PUT: api/Settings/tenant/{tenantId}
    [HttpPut("tenant/{tenantId}")]
    public async Task<ActionResult> UpdateTenantSettings(Guid tenantId, [FromBody] TenantSettingsDto dto)
    {
        await _settingRepository.UpdateTenantSettingsAsync(tenantId, dto);
        return Ok(new { success = true, message = "Impostazioni aggiornate" });
    }

    // GET: api/Settings/types
    [HttpGet("types")]
    public async Task<ActionResult> GetSettingTypes()
    {
        var types = await _settingRepository.GetSettingTypesAsync();
        var typeDtos = types.Select(t => new SettingTypeDto { Id = t.Id, Name = t.Name });
        return Ok(new { success = true, data = typeDtos });
    }

    // GET: api/Settings
    [HttpGet]
    public async Task<ActionResult> GetSettings([FromQuery] Guid? tenantId)
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

    // POST: api/Settings
    [HttpPost]
    public async Task<ActionResult> CreateSetting([FromBody] CreateSettingDto dto)
    {
        var setting = new Setting
        {
            TenantId = dto.TenantId,
            SettingTypeId = dto.SettingTypeId,
            Value = dto.Value
        };
        var createdSetting = await _settingRepository.CreateSettingAsync(setting);
        return CreatedAtAction(nameof(GetSettings), new { tenantId = createdSetting.TenantId }, new { success = true, data = new { id = createdSetting.Id } });
    }

    // PUT: api/Settings/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateSetting(int id, [FromBody] UpdateSettingDto dto)
    {
        var setting = new Setting { Id = id, Value = dto.Value };
        await _settingRepository.UpdateSettingAsync(setting);
        return Ok(new { success = true, message = "Impostazione aggiornata" });
    }

    // DELETE: api/Settings/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSetting(int id)
    {
        await _settingRepository.DeleteSettingAsync(id);
        return Ok(new { success = true, message = "Impostazione eliminata" });
    }
}