using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminRepository _adminRepository;

    public AdminController(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        try
        {
            var stats = await _adminRepository.GetAdminStatsAsync();
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
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
                return BadRequest(new { success = false, error = "Query parameter 'q' is required" });
            }

            var results = await _adminRepository.GlobalSearchAsync(q, limit);
            return Ok(new { success = true, data = results });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = $"Errore durante la ricerca: {ex.Message}"
            });
        }
    }
}