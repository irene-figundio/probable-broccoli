using WorkBotAI.API.DTOs;
using WorkBotAI.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using WorkBotAI.Repositories.DTOs;

namespace WorkBotAI.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminRepository _adminRepository;
    private readonly IAuditService _auditService;

    public AdminController(IAdminRepository adminRepository, IAuditService auditService)
    {
        _adminRepository = adminRepository;
        _auditService = auditService;
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        try
        {
            var stats = await _adminRepository.GetAdminStatsAsync();
            if (stats == null)
            {
                await _auditService.LogActionAsync("Admin", "GetStats", "No statistics found");
                return Ok(new { success = true, data = new AdminStatsDto() });
            }
            await _auditService.LogActionAsync("Admin", "GetStats", "Retrieved admin statistics");
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Admin", "GetStats", ex);
            return StatusCode(500, new
            {
                success = false,
                error = $"Errore nel recupero delle statistiche: {ex.Message}"
            });
        }
    }
    [HttpGet("stats/{id}")]
    public async Task<ActionResult> GetStatsById(Guid id)
    {
        try
        {
            var stats = await _adminRepository.GetAdminStatsByIdAsync(id);
            if (stats == null)
            {
                await _auditService.LogActionAsync("Admin", "GetStatsById", "No statistics found");
                return Ok(new { success = true, data = new AdminTenantStatsDto() });
            }
            await _auditService.LogActionAsync("Admin", "GetStatsById", "Retrieved admin statistics");
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Admin", "GetStatsById", ex);
            return StatusCode(500, new
            {
                success = false,
                error = $"Errore nel recupero delle statistiche: {ex.Message}"
            });
        }
    }
    [HttpGet("search")]
    public async Task<ActionResult> GlobalSearch([FromQuery] string q, [FromQuery] int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                await _auditService.LogErrorAsync("Admin - GlobalSearch", "Query parameter 'q' is required", null, null, null);
                return BadRequest(new { success = false, error = "Query parameter 'q' is required" });
            }

            var results = await _adminRepository.GlobalSearchAsync(q, limit);
            if (results == null || results.TotalResults == 0)
            {
                await _auditService.LogActionAsync("Admin", "GlobalSearch", $"No results found for query: {q}");
                return Ok(new { success = true, data = results });
            }
            await _auditService.LogActionAsync("Admin", "GlobalSearch", $"Search query: {q}");
            return Ok(new { success = true, data = results });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Admin", "Error during global search", ex);
            return StatusCode(500, new
            {
                success = false,
                error = $"Errore durante la ricerca: {ex.Message}"
            });
        }
    }
}