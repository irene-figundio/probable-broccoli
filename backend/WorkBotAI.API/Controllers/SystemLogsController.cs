using WorkBotAI.API.DTOs;
using WorkBotAI.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemLogsController : ControllerBase
{
    private readonly ISystemLogRepository _repository;
    private readonly ILogger<SystemLogsController> _logger;
    private readonly IAuditService _auditService;
    public SystemLogsController(ISystemLogRepository repository, ILogger<SystemLogsController> logger, IAuditService auditService)
    {
        _repository = repository;
        _logger = logger;
        _auditService = auditService;
    }

    // GET: api/SystemLogs
    [HttpGet]
    public async Task<ActionResult> GetLogs(
        [FromQuery] string? level,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? tenantId,
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // Validazione input
            if (page < 1) 
            { 
                await _auditService.LogActionAsync("GetLogs", "Invalid page number", HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Headers["User-Agent"].ToString(), null);
                return BadRequest(new { success = false, error = "La pagina deve essere >= 1" }); 
            }
            if (pageSize < 1 || pageSize > 500) 
            { 
                await _auditService.LogActionAsync("GetLogs", "Invalid page size", HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Headers.UserAgent.ToString(), null);
                return BadRequest(new { success = false, error = "pageSize non valido (1-500)" }); 
            } 
            if (startDate == null) 
            { 
                startDate = DateTime.UtcNow.AddDays(-7); // Default a ultimi 7 giorni
            }
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                startDate = startDate.Value;
                endDate = endDate.Value;
                await _auditService.LogActionAsync("GetLogs", "Invalid date range", HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Headers["User-Agent"].ToString(), null);
                return BadRequest(new { success = false, error = "La data di inizio non può essere successiva alla data di fine" });
            }

            var (logs, totalCount) = await _repository.GetLogsAsync(level, startDate, endDate, tenantId, searchTerm, page, pageSize);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var logsDto = logs.Select(l => new SystemLogDto
            {
                Id = l.Id,
                Timestamp = l.Timestamp,
                Level = l.Level,
                Source = l.Source,
                Message = l.Message,
                Context = l.Context,
                UserId = l.UserId,
                TenantId = l.TenantId,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent
            }).ToList();

            return Ok(new
            {
                success = true,
                data = new LogsResponseDto
                {
                    Logs = logsDto,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero dei log di sistema");
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR", message = ex.Message });
        }
    }

    // GET: api/SystemLogs/5
    [HttpGet("{id}")]
    public async Task<ActionResult> GetLog(int id)
    {
        try
        {
            var log = await _repository.GetLogByIdAsync(id);

            if (log == null)
            {
                await _auditService.LogActionAsync("GetLog", "Log not found", HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Headers["User-Agent"].ToString(), null);
                return NotFound(new { success = false, error = "Log non trovato" });
            }

            return Ok(new
            {
                success = true,
                data = new SystemLogDto
                {
                    Id = log.Id,
                    Timestamp = log.Timestamp,
                    Level = log.Level,
                    Source = log.Source,
                    Message = log.Message,
                    Context = log.Context,
                    UserId = log.UserId,
                    TenantId = log.TenantId,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero del log {Id}", id);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/SystemLogs
    [HttpPost]
    public async Task<ActionResult> CreateLog([FromBody] CreateLogDto dto)
    {
        try
        {
            var log = new SystemLog
            {
                Timestamp = DateTime.UtcNow,
                Level = dto.Level,
                Source = dto.Source,
                Message = dto.Message,
                Context = dto.Context,
                UserId = dto.UserId,
                TenantId = dto.TenantId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            await _repository.CreateLogAsync(log);

            return CreatedAtAction(nameof(GetLog), new { id = log.Id },
                new { success = true, data = new { id = log.Id } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nella creazione del log");
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/SystemLogs/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLog(int id)
    {
        try
        {
            var deleted = await _repository.DeleteLogAsync(id);

            if (!deleted)
            {
                await _auditService.LogActionAsync("DeleteLog", "Log not found", HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Headers["User-Agent"].ToString(), null);
                return NotFound(new { success = false, error = "Log non trovato" });
            }

            return Ok(new { success = true, message = "Log eliminato" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'eliminazione del log {Id}", id);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/SystemLogs/clear
    [HttpDelete("clear")]
    public async Task<ActionResult> ClearLogs([FromQuery] DateTime? olderThan)
    {
        try
        {
            var count = await _repository.ClearLogsAsync(olderThan);
            if (count == 0)
            {
                await _auditService.LogActionAsync("ClearLogs", "No logs to delete", HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Headers["User-Agent"].ToString(), null);
                return Ok(new { success = true, message = "Nessun log da eliminare" });
            }
            await _auditService.LogActionAsync("ClearLogs", $"{count} log eliminati", HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Headers["User-Agent"].ToString(), null);
            return Ok(new { success = true, message = $"{count} log eliminati" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nella pulizia dei log");
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/SystemLogs/stats
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        try
        {
            var stats = await _repository.GetStatsAsync();
            if (stats == null)
                {
                await _auditService.LogActionAsync("GetStats", "No statistics found", HttpContext.Connection.RemoteIpAddress?.ToString(), HttpContext.Request.Headers["User-Agent"].ToString(), null);
                return Ok(new { success = true, data = new object() });
            }
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero delle statistiche dei log");
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/SystemLogs/seed - Per popolare con dati di test
    //[HttpPost("seed")]
    //public async Task<ActionResult> SeedLogs()
    //{
    //    try
    //    {
    //        var sources = new[] { "AuthController", "AppointmentsController", "TenantsController", "UsersController", "System" };
    //        var levels = new[] { "info", "warning", "error", "debug" };
    //        var messages = new Dictionary<string, string[]>
    //        {
    //            { "info", new[] { "Utente loggato con successo", "Appuntamento creato", "Tenant registrato", "Impostazioni aggiornate", "Email inviata" } },
    //            { "warning", new[] { "Tentativo di accesso fallito", "Sessione in scadenza", "Quota API quasi esaurita", "Backup ritardato" } },
    //            { "error", new[] { "Errore connessione database", "Timeout API esterna", "Errore invio email", "Validazione fallita" } },
    //            { "debug", new[] { "Query eseguita in 50ms", "Cache invalidata", "Token refreshato", "Webhook ricevuto" } }
    //        };

    //        var random = new Random();
    //        var logs = new List<SystemLog>();

    //        for (int i = 0; i < 100; i++)
    //        {
    //            var level = levels[random.Next(levels.Length)];
    //            var log = new SystemLog
    //            {
    //                Timestamp = DateTime.UtcNow.AddHours(-random.Next(0, 168)), // Ultimi 7 giorni
    //                Level = level,
    //                Source = sources[random.Next(sources.Length)],
    //                Message = messages[level][random.Next(messages[level].Length)],
    //                Context = random.Next(2) == 0 ? null : $"{{\"requestId\": \"{Guid.NewGuid()}\", \"duration\": {random.Next(10, 500)}}}",
    //                UserId = random.Next(2) == 0 ? null : random.Next(1, 4),
    //                TenantId = random.Next(2) == 0 ? null : Guid.Parse("7e0429b6-dca7-4e4c-aaf1-388b97bc3512"),
    //                IpAddress = $"192.168.1.{random.Next(1, 255)}",
    //                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
    //            };
    //            logs.Add(log);
    //        }

    //        foreach (var log in logs)
    //        {
    //            await _repository.CreateLogAsync(log);
    //        }

    //        return Ok(new { success = true, message = $"{logs.Count} log di test creati" });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Errore durante il seeding dei log");
    //        return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
    //    }
    //}
}
