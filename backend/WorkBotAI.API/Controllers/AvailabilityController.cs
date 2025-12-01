using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public AvailabilityController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Availability
    [HttpGet]
    public async Task<ActionResult> GetAvailabilities(
        [FromQuery] Guid? tenantId,
        [FromQuery] int? staffId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.Availabilities
            .Include(a => a.Staff)
            .Include(a => a.Tenant)
            .AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(a => a.TenantId == tenantId.Value);

        if (staffId.HasValue)
            query = query.Where(a => a.StaffId == staffId.Value);

        if (from.HasValue)
            query = query.Where(a => a.EndTime >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.StartTime <= to.Value);

        var availabilities = await query
            .OrderBy(a => a.StartTime)
            .Select(a => new AvailabilityListDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                StaffId = a.StaffId,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note
            })
            .ToListAsync();

        return Ok(new { success = true, data = availabilities });
    }

    // GET: api/Availability/5
    [HttpGet("{id}")]
    public async Task<ActionResult> GetAvailability(int id)
    {
        var availability = await _context.Availabilities
            .Include(a => a.Staff)
            .Include(a => a.Tenant)
            .Where(a => a.Id == id)
            .Select(a => new AvailabilityDetailDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                TenantName = a.Tenant.Name,
                StaffId = a.StaffId,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note
            })
            .FirstOrDefaultAsync();

        if (availability == null)
            return NotFound(new { success = false, error = "Disponibilità non trovata" });

        return Ok(new { success = true, data = availability });
    }

    // POST: api/Availability
    [HttpPost]
    public async Task<ActionResult> CreateAvailability([FromBody] CreateAvailabilityDto dto)
    {
        // Validazione
        if (dto.StartTime >= dto.EndTime)
            return BadRequest(new { success = false, error = "L'orario di inizio deve essere prima dell'orario di fine" });

        // Verifica tenant esiste
        var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == dto.TenantId);
        if (!tenantExists)
            return BadRequest(new { success = false, error = "Tenant non trovato" });

        // Verifica staff esiste (se specificato)
        if (dto.StaffId.HasValue)
        {
            var staffExists = await _context.Staff.AnyAsync(s => s.Id == dto.StaffId.Value && s.TenantId == dto.TenantId);
            if (!staffExists)
                return BadRequest(new { success = false, error = "Staff non trovato per questo tenant" });
        }

        // Verifica sovrapposizioni
        var hasOverlap = await _context.Availabilities
            .Where(a => a.TenantId == dto.TenantId)
            .Where(a => dto.StaffId == null || a.StaffId == dto.StaffId)
            .Where(a => a.StartTime < dto.EndTime && a.EndTime > dto.StartTime)
            .AnyAsync();

        if (hasOverlap)
            return BadRequest(new { success = false, error = "Esiste già una disponibilità in questo orario" });

        var availability = new Availability
        {
            TenantId = dto.TenantId,
            StaffId = dto.StaffId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Note = dto.Note
        };

        _context.Availabilities.Add(availability);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAvailability), new { id = availability.Id }, 
            new { success = true, data = new { id = availability.Id } });
    }

    // PUT: api/Availability/5
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAvailability(int id, [FromBody] UpdateAvailabilityDto dto)
    {
        var availability = await _context.Availabilities.FindAsync(id);
        if (availability == null)
            return NotFound(new { success = false, error = "Disponibilità non trovata" });

        // Validazione
        if (dto.StartTime >= dto.EndTime)
            return BadRequest(new { success = false, error = "L'orario di inizio deve essere prima dell'orario di fine" });

        // Verifica staff esiste (se specificato)
        if (dto.StaffId.HasValue)
        {
            var staffExists = await _context.Staff.AnyAsync(s => s.Id == dto.StaffId.Value && s.TenantId == availability.TenantId);
            if (!staffExists)
                return BadRequest(new { success = false, error = "Staff non trovato per questo tenant" });
        }

        // Verifica sovrapposizioni (escludendo se stesso)
        var hasOverlap = await _context.Availabilities
            .Where(a => a.TenantId == availability.TenantId)
            .Where(a => a.Id != id)
            .Where(a => dto.StaffId == null || a.StaffId == dto.StaffId)
            .Where(a => a.StartTime < dto.EndTime && a.EndTime > dto.StartTime)
            .AnyAsync();

        if (hasOverlap)
            return BadRequest(new { success = false, error = "Esiste già una disponibilità in questo orario" });

        availability.StaffId = dto.StaffId;
        availability.StartTime = dto.StartTime;
        availability.EndTime = dto.EndTime;
        availability.Note = dto.Note;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Disponibilità aggiornata" });
    }

    // DELETE: api/Availability/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAvailability(int id)
    {
        var availability = await _context.Availabilities.FindAsync(id);
        if (availability == null)
            return NotFound(new { success = false, error = "Disponibilità non trovata" });

        _context.Availabilities.Remove(availability);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Disponibilità eliminata" });
    }

    // GET: api/Availability/slots - Slot disponibili per prenotazione
    [HttpGet("slots")]
    public async Task<ActionResult> GetAvailableSlots(
        [FromQuery] Guid tenantId,
        [FromQuery] DateTime date,
        [FromQuery] int? serviceId,
        [FromQuery] int? staffId)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);

        // Ottieni durata servizio (default 30 min)
        var serviceDuration = 30;
        if (serviceId.HasValue)
        {
            var service = await _context.Services.FindAsync(serviceId.Value);
            if (service != null)
                serviceDuration = service.DurationMin ?? 30;
        }

        // Ottieni disponibilità del giorno
        var availabilities = await _context.Availabilities
            .Include(a => a.Staff)
            .Where(a => a.TenantId == tenantId)
            .Where(a => !staffId.HasValue || a.StaffId == staffId)
            .Where(a => a.StartTime < endOfDay && a.EndTime > startOfDay)
            .ToListAsync();

        // Ottieni appuntamenti già prenotati
        var appointments = await _context.Appointments
            .Where(a => a.TenantId == tenantId)
            .Where(a => !staffId.HasValue || a.StaffId == staffId)
            .Where(a => a.StartTime < endOfDay && a.EndTime > startOfDay)
            .Where(a => a.StatusId != 4) // Escludi cancellati
            .ToListAsync();

        var slots = new List<AvailableSlotDto>();

        foreach (var avail in availabilities)
        {
            var currentTime = avail.StartTime < startOfDay ? startOfDay : avail.StartTime;
            var endTime = avail.EndTime > endOfDay ? endOfDay : avail.EndTime;

            while (currentTime.AddMinutes(serviceDuration) <= endTime)
            {
                var slotEnd = currentTime.AddMinutes(serviceDuration);

                // Verifica se lo slot è libero
                var isBooked = appointments.Any(app =>
                    app.StaffId == avail.StaffId &&
                    app.StartTime < slotEnd &&
                    app.EndTime > currentTime);

                if (!isBooked)
                {
                    slots.Add(new AvailableSlotDto
                    {
                        StaffId = avail.StaffId ?? 0,
                        StaffName = avail.Staff?.Name ?? "Qualsiasi",
                        StartTime = currentTime,
                        EndTime = slotEnd
                    });
                }

                currentTime = currentTime.AddMinutes(serviceDuration);
            }
        }

        return Ok(new { success = true, data = slots.OrderBy(s => s.StartTime).ThenBy(s => s.StaffName) });
    }

    // GET: api/Availability/week - Disponibilità settimanale
    [HttpGet("week")]
    public async Task<ActionResult> GetWeekAvailability(
        [FromQuery] Guid tenantId,
        [FromQuery] DateTime startDate)
    {
        var endDate = startDate.AddDays(7);

        var availabilities = await _context.Availabilities
            .Include(a => a.Staff)
            .Where(a => a.TenantId == tenantId)
            .Where(a => a.StartTime < endDate && a.EndTime > startDate)
            .OrderBy(a => a.StartTime)
            .Select(a => new AvailabilityListDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                StaffId = a.StaffId,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note
            })
            .ToListAsync();

        return Ok(new { success = true, data = availabilities });
    }
}