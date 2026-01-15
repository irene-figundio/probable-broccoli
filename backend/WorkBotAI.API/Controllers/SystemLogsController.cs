using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemLogsController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public SystemLogsController(WorkBotAIContext context)
    {
        _context = context;
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
        var query = _context.SystemLogs.AsQueryable();

        // Filtri
        if (!string.IsNullOrEmpty(level) && level != "all")
            query = query.Where(l => l.Level.ToLower() == level.ToLower());

        if (startDate.HasValue)
            query = query.Where(l => l.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.Timestamp <= endDate.Value.AddDays(1));

        if (tenantId.HasValue)
            query = query.Where(l => l.TenantId == tenantId.Value);

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(l => 
                l.Message.Contains(searchTerm) || 
                l.Source.Contains(searchTerm) ||
                (l.Context != null && l.Context.Contains(searchTerm)));

        // Conteggio totale
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Paginazione e ordinamento
        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new SystemLogDto
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
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new LogsResponseDto
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            }
        });
    }

    // GET: api/SystemLogs/5
    [HttpGet("{id}")]
    public async Task<ActionResult> GetLog(int id)
    {
        var log = await _context.SystemLogs.FindAsync(id);

        if (log == null)
            return NotFound(new { success = false, error = "Log non trovato" });

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

    // POST: api/SystemLogs
    [HttpPost]
    public async Task<ActionResult> CreateLog([FromBody] CreateLogDto dto)
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

        _context.SystemLogs.Add(log);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLog), new { id = log.Id },
            new { success = true, data = new { id = log.Id } });
    }

    // DELETE: api/SystemLogs/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLog(int id)
    {
        var log = await _context.SystemLogs.FindAsync(id);
        if (log == null)
            return NotFound(new { success = false, error = "Log non trovato" });

        _context.SystemLogs.Remove(log);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Log eliminato" });
    }

    // DELETE: api/SystemLogs/clear
    [HttpDelete("clear")]
    public async Task<ActionResult> ClearLogs([FromQuery] DateTime? olderThan)
    {
        var query = _context.SystemLogs.AsQueryable();

        if (olderThan.HasValue)
            query = query.Where(l => l.Timestamp < olderThan.Value);

        var count = await query.CountAsync();
        _context.SystemLogs.RemoveRange(query);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = $"{count} log eliminati" });
    }

    // GET: api/SystemLogs/stats
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);

        var stats = new
        {
            totalLogs = await _context.SystemLogs.CountAsync(),
            todayLogs = await _context.SystemLogs.CountAsync(l => l.Timestamp >= today),
            weekLogs = await _context.SystemLogs.CountAsync(l => l.Timestamp >= weekAgo),
            errorCount = await _context.SystemLogs.CountAsync(l => l.Level == "error"),
            warningCount = await _context.SystemLogs.CountAsync(l => l.Level == "warning"),
            infoCount = await _context.SystemLogs.CountAsync(l => l.Level == "info"),
            byLevel = await _context.SystemLogs
                .GroupBy(l => l.Level)
                .Select(g => new { level = g.Key, count = g.Count() })
                .ToListAsync()
        };

        return Ok(new { success = true, data = stats });
    }

    // POST: api/SystemLogs/seed - Per popolare con dati di test
    [HttpPost("seed")]
    public async Task<ActionResult> SeedLogs()
    {
        var sources = new[] { "AuthController", "AppointmentsController", "TenantsController", "UsersController", "System" };
        var levels = new[] { "info", "warning", "error", "debug" };
        var messages = new Dictionary<string, string[]>
        {
            { "info", new[] { "Utente loggato con successo", "Appuntamento creato", "Tenant registrato", "Impostazioni aggiornate", "Email inviata" } },
            { "warning", new[] { "Tentativo di accesso fallito", "Sessione in scadenza", "Quota API quasi esaurita", "Backup ritardato" } },
            { "error", new[] { "Errore connessione database", "Timeout API esterna", "Errore invio email", "Validazione fallita" } },
            { "debug", new[] { "Query eseguita in 50ms", "Cache invalidata", "Token refreshato", "Webhook ricevuto" } }
        };

        var random = new Random();
        var logs = new List<SystemLog>();

        for (int i = 0; i < 100; i++)
        {
            var level = levels[random.Next(levels.Length)];
            var log = new SystemLog
            {
                Timestamp = DateTime.UtcNow.AddHours(-random.Next(0, 168)), // Ultimi 7 giorni
                Level = level,
                Source = sources[random.Next(sources.Length)],
                Message = messages[level][random.Next(messages[level].Length)],
                Context = random.Next(2) == 0 ? null : $"{{\"requestId\": \"{Guid.NewGuid()}\", \"duration\": {random.Next(10, 500)}}}",
                UserId = random.Next(2) == 0 ? null : random.Next(1, 4),
                TenantId = random.Next(2) == 0 ? null : Guid.Parse("7e0429b6-dca7-4e4c-aaf1-388b97bc3512"),
                IpAddress = $"192.168.1.{random.Next(1, 255)}",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
            };
            logs.Add(log);
        }

        _context.SystemLogs.AddRange(logs);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = $"{logs.Count} log di test creati" });
    }
}