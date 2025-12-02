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
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityRepository _availabilityRepository;

    public AvailabilityController(IAvailabilityRepository availabilityRepository)
    {
        _availabilityRepository = availabilityRepository;
    }

    // GET: api/Availability
    [HttpGet]
    public async Task<ActionResult> GetAvailabilities(
        [FromQuery] Guid? tenantId,
        [FromQuery] int? staffId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var availabilities = await _availabilityRepository.GetAvailabilitiesAsync(tenantId, staffId, from, to);
        var availabilityDtos = availabilities.Select(a => new AvailabilityListDto
        {
            Id = a.Id,
            TenantId = a.TenantId,
            StaffId = a.StaffId,
            StaffName = a.Staff != null ? a.Staff.Name : null,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Note = a.Note
        });
        return Ok(new { success = true, data = availabilityDtos });
    }

    // GET: api/Availability/5
    [HttpGet("{id}")]
    public async Task<ActionResult> GetAvailability(int id)
    {
        var availability = await _availabilityRepository.GetAvailabilityByIdAsync(id);
        if (availability == null)
            return NotFound(new { success = false, error = "Disponibilità non trovata" });

        var availabilityDto = new AvailabilityDetailDto
        {
            Id = availability.Id,
            TenantId = availability.TenantId,
            TenantName = availability.Tenant.Name,
            StaffId = availability.StaffId,
            StaffName = availability.Staff != null ? availability.Staff.Name : null,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime,
            Note = availability.Note
        };
        return Ok(new { success = true, data = availabilityDto });
    }

    // POST: api/Availability
    [HttpPost]
    public async Task<ActionResult> CreateAvailability([FromBody] CreateAvailabilityDto dto)
    {
        if (dto.StartTime >= dto.EndTime)
            return BadRequest(new { success = false, error = "L'orario di inizio deve essere prima dell'orario di fine" });

        var availability = new Availability
        {
            TenantId = dto.TenantId,
            StaffId = dto.StaffId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Note = dto.Note
        };

        var createdAvailability = await _availabilityRepository.CreateAvailabilityAsync(availability);
        return CreatedAtAction(nameof(GetAvailability), new { id = createdAvailability.Id }, new { success = true, data = new { id = createdAvailability.Id } });
    }

    // PUT: api/Availability/5
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAvailability(int id, [FromBody] UpdateAvailabilityDto dto)
    {
        var availability = await _availabilityRepository.GetAvailabilityByIdAsync(id);
        if (availability == null)
            return NotFound(new { success = false, error = "Disponibilità non trovata" });

        if (dto.StartTime >= dto.EndTime)
            return BadRequest(new { success = false, error = "L'orario di inizio deve essere prima dell'orario di fine" });

        availability.StaffId = dto.StaffId;
        availability.StartTime = dto.StartTime;
        availability.EndTime = dto.EndTime;
        availability.Note = dto.Note;

        await _availabilityRepository.UpdateAvailabilityAsync(availability);
        return Ok(new { success = true, message = "Disponibilità aggiornata" });
    }

    // DELETE: api/Availability/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAvailability(int id)
    {
        await _availabilityRepository.DeleteAvailabilityAsync(id);
        return Ok(new { success = true, message = "Disponibilità eliminata" });
    }

    // GET: api/Availability/slots
    [HttpGet("slots")]
    public async Task<ActionResult> GetAvailableSlots([FromQuery] Guid tenantId, [FromQuery] DateTime date, [FromQuery] int? serviceId, [FromQuery] int? staffId)
    {
        var slots = await _availabilityRepository.GetAvailableSlotsAsync(tenantId, date, serviceId, staffId);
        return Ok(new { success = true, data = slots });
    }

    // GET: api/Availability/week
    [HttpGet("week")]
    public async Task<ActionResult> GetWeekAvailability([FromQuery] Guid tenantId, [FromQuery] DateTime startDate)
    {
        var availabilities = await _availabilityRepository.GetWeekAvailabilityAsync(tenantId, startDate);
        var availabilityDtos = availabilities.Select(a => new AvailabilityListDto
        {
            Id = a.Id,
            TenantId = a.TenantId,
            StaffId = a.StaffId,
            StaffName = a.Staff != null ? a.Staff.Name : null,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Note = a.Note
        });
        return Ok(new { success = true, data = availabilityDtos });
    }
}