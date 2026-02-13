using WorkBotAI.API.DTOs;
using WorkBotAI.API.Services;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(ISystemSettingRepository repository, IAuditService auditService, ILogger<SystemSettingsController> logger)
    {
        _repository = repository;
        _auditService = auditService;
        _logger = logger;
    }

    // GET: api/SystemSettings
    [HttpGet]
    public async Task<ActionResult> GetAllSettings()
    {
        try
        {
            var settings = await _repository.GetAllSettingsAsync();

            // Raggruppa per categoria
            var grouped = settings
                .GroupBy(s => s.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(s => s.Key, s => s.Value)
                );

            return Ok(new { success = true, data = grouped });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero delle impostazioni di sistema");
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/SystemSettings/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult> GetByCategory(string category)
    {
        try
        {
            var settings = await _repository.GetByCategoryAsync(category);
            var result = settings.ToDictionary(s => s.Key, s => s.Value);

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero delle impostazioni per la categoria {Category}", category);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/SystemSettings/{category}/{key}
    [HttpGet("{category}/{key}")]
    public async Task<ActionResult> GetSetting(string category, string key)
    {
        try
        {
            var setting = await _repository.GetSettingAsync(category, key);

            if (setting == null)
                return NotFound(new { success = false, error = "Impostazione non trovata" });

            return Ok(new { success = true, data = setting.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero dell'impostazione {Category}/{Key}", category, key);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/SystemSettings
    [HttpPut]
    public async Task<ActionResult> UpdateSettings([FromBody] Dictionary<string, Dictionary<string, string?>> settings)
    {
        try
        {
            await _repository.UpdateSettingsAsync(settings);
            await _auditService.LogActionAsync("SystemSettings", "UpdateAll", "Updated multiple system settings");
            return Ok(new { success = true, message = "Impostazioni aggiornate" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'aggiornamento delle impostazioni");
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/SystemSettings/{category}/{key}
    [HttpPut("{category}/{key}")]
    public async Task<ActionResult> UpdateSetting(string category, string key, [FromBody] string? value)
    {
        try
        {
            await _repository.UpdateSettingAsync(category, key, value);
            await _auditService.LogActionAsync("SystemSettings", "Update", $"Updated setting {category}/{key} to {value}");
            return Ok(new { success = true, message = "Impostazione aggiornata" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'aggiornamento dell'impostazione {Category}/{Key}", category, key);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/SystemSettings/seed
    [HttpPost("seed")]
    public async Task<ActionResult> SeedSettings()
    {
        try
        {
            var settings = await _repository.GetAllSettingsAsync();
            if (settings.Any())
            {
                return BadRequest(new { success = false, error = "Impostazioni già presenti" });
            }

            await _repository.SeedDefaultSettingsAsync();
            return Ok(new { success = true, message = "Impostazioni di default create" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel seeding delle impostazioni");
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
