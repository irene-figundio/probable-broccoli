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
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly IAuditService _auditService;

    public AvailabilityController(IAvailabilityRepository availabilityRepository, IAuditService auditService)
    {
        _availabilityRepository = availabilityRepository;
        _auditService = auditService;
    }

    // GET: api/Availability
    [HttpGet]
    public async Task<ActionResult> GetAvailabilities(
        [FromQuery] Guid? tenantId,
        [FromQuery] int? staffId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
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
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Availability", "Error retrieving availabilities", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Availability/5
    [HttpGet("{id}")]
    public async Task<ActionResult> GetAvailability(int id)
    {
        try
        {
            var availability = await _availabilityRepository.GetAvailabilityByIdAsync(id);
            if (availability == null)
            {
                await _auditService.LogErrorAsync("Availability - GetAvailability", $"Availability not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Disponibilità non trovata" });
            } else {
                await _auditService.LogActionAsync("Availability", "Get", $"Retrieved availability {id}", null, availability.TenantId);
            }

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
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Availability", $"Error retrieving availability {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/Availability
    [HttpPost]
    public async Task<ActionResult> CreateAvailability([FromBody] CreateAvailabilityDto dto)
    {
        try
        {
            if (dto.StartTime >= dto.EndTime)
            {
                await _auditService.LogErrorAsync("Availability - CreateAvailability", "Invalid time range", null, dto.TenantId);
                return BadRequest(new { success = false, error = "L'orario di inizio deve essere prima dell'orario di fine" });
            }   else {  
                await _auditService.LogActionAsync("Availability", "Create Attempt", $"Attempting to create availability for staff {dto.StaffId}", null, dto.TenantId);
            }

            var availability = new Availability
            {
                TenantId = dto.TenantId,
                StaffId = dto.StaffId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Note = dto.Note
            };

            var createdAvailability = await _availabilityRepository.CreateAvailabilityAsync(availability);
            await _auditService.LogActionAsync("Availability", "Create", $"Created availability {createdAvailability.Id} for staff {dto.StaffId}", null, dto.TenantId);

            return CreatedAtAction(nameof(GetAvailability), new { id = createdAvailability.Id }, new { success = true, data = new { id = createdAvailability.Id } });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Availability", "Error creating availability", ex, dto.TenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Availability/5
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAvailability(int id, [FromBody] UpdateAvailabilityDto dto)
    {
        try
        {
            var availability = await _availabilityRepository.GetAvailabilityByIdAsync(id);
            if (availability == null)
            {
                await _auditService.LogErrorAsync("Availability - UpdateAvailability", $"Availability not found: {id}", null, null, null);      
                return NotFound(new { success = false, error = "Disponibilità non trovata" });
            } else {
                await _auditService.LogActionAsync("Availability", "Update Attempt", $"Attempting to update availability {id} for staff {dto.StaffId}", null, availability.TenantId);
            }

            if (dto.StartTime >= dto.EndTime)
            {
                await _auditService.LogErrorAsync("Availability - UpdateAvailability", "Invalid time range", null, availability.TenantId);
                return BadRequest(new { success = false, error = "L'orario di inizio deve essere prima dell'orario di fine" });
            }               

            availability.StaffId = dto.StaffId;
            availability.StartTime = dto.StartTime;
            availability.EndTime = dto.EndTime;
            availability.Note = dto.Note;

            await _availabilityRepository.UpdateAvailabilityAsync(availability);
            await _auditService.LogActionAsync("Availability", "Update", $"Updated availability {id}", null, availability.TenantId);

            return Ok(new { success = true, message = "Disponibilità aggiornata" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Availability", $"Error updating availability {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/Availability/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAvailability(int id)
    {
        try
        {
            var availability = await _availabilityRepository.GetAvailabilityByIdAsync(id);
            if (availability == null)
            {
                await _auditService.LogErrorAsync("Availability - DeleteAvailability", $"Availability not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Disponibilità non trovata" });
            }

            await _availabilityRepository.DeleteAvailabilityAsync(id);
            await _auditService.LogActionAsync("Availability", "Delete", $"Deleted availability {id}", null, availability.TenantId);
            return Ok(new { success = true, message = "Disponibilità eliminata" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Availability", $"Error deleting availability {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Availability/slots
    [HttpGet("slots")]
    public async Task<ActionResult> GetAvailableSlots([FromQuery] Guid tenantId, [FromQuery] DateTime date, [FromQuery] int? serviceId, [FromQuery] int? staffId)
    {
        try
        {
            var slots = await _availabilityRepository.GetAvailableSlotsAsync(tenantId, date, serviceId, staffId);
            return Ok(new { success = true, data = slots });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Availability", "Error retrieving available slots", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Availability/week
    [HttpGet("week")]
    public async Task<ActionResult> GetWeekAvailability([FromQuery] Guid tenantId, [FromQuery] DateTime startDate)
    {
        try
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
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Availability", "Error retrieving week availability", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
