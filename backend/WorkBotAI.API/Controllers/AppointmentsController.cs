using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public AppointmentsController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Appointments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentListDto>>> GetAppointments(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] int? statusId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = _context.Appointments
            .Where(a => a.IsDeleted != true)
            .Include(a => a.Tenant)
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Status)
            .Include(a => a.AppointmentServices)
                .ThenInclude(asvc => asvc.Service)
            .AsQueryable();

        // Filtro per tenant
        if (tenantId.HasValue)
        {
            query = query.Where(a => a.TenantId == tenantId.Value);
        }

        // Filtro per stato
        if (statusId.HasValue)
        {
            query = query.Where(a => a.StatusId == statusId.Value);
        }

        // Filtro per data inizio
        if (fromDate.HasValue)
        {
            query = query.Where(a => a.StartTime >= fromDate.Value);
        }

        // Filtro per data fine
        if (toDate.HasValue)
        {
            query = query.Where(a => a.StartTime <= toDate.Value);
        }

        var appointments = await query
            .OrderByDescending(a => a.StartTime)
            .Select(a => new AppointmentListDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                TenantName = a.Tenant.Name,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer != null ? a.Customer.FullName : null,
                StaffId = a.StaffId,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                ServiceId = a.AppointmentServices.FirstOrDefault() != null ? a.AppointmentServices.FirstOrDefault()!.ServiceId : null,
                ServiceName = a.AppointmentServices.FirstOrDefault() != null && a.AppointmentServices.FirstOrDefault()!.Service != null 
                    ? a.AppointmentServices.FirstOrDefault()!.Service!.Name : null,
                TotalPrice = a.AppointmentServices.Sum(s => s.Service != null ? s.Service.BasePrice : 0),
                StatusId = a.StatusId,
                StatusName = a.Status != null ? a.Status.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note,
                IsActive = a.IsActive ?? false
            })
            .ToListAsync();

        return Ok(new { success = true, data = appointments });
    }

    // GET: api/Appointments/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetAppointment(int id)
    {
        var appointment = await _context.Appointments
            .Where(a => a.Id == id && a.IsDeleted != true)
            .Include(a => a.Tenant)
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Resource)
            .Include(a => a.Status)
            .Include(a => a.AppointmentServices)
                .ThenInclude(asvc => asvc.Service)
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return NotFound(new { success = false, error = "Appuntamento non trovato" });
        }

        var result = new AppointmentDetailDto
        {
            Id = appointment.Id,
            TenantId = appointment.TenantId,
            TenantName = appointment.Tenant.Name,
            CustomerId = appointment.CustomerId,
            CustomerName = appointment.Customer?.FullName,
            CustomerPhone = appointment.Customer?.Phone,
            CustomerEmail = appointment.Customer?.Email,
            StaffId = appointment.StaffId,
            StaffName = appointment.Staff?.Name,
            ResourceId = appointment.ResourceId,
            ResourceName = appointment.Resource?.Code,
            StatusId = appointment.StatusId,
            StatusName = appointment.Status?.Name,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Note = appointment.Note,
            IsActive = appointment.IsActive ?? false,
            CreationTime = appointment.CreationTime,
            LastModificationTime = appointment.LastModificationTime,
            Services = appointment.AppointmentServices.Select(asvc => new AppointmentServiceDto
            {
                Id = asvc.Id,
                ServiceId = asvc.ServiceId,
                ServiceName = asvc.Service?.Name ?? "",
                Price = asvc.Service?.BasePrice,
                Duration = asvc.Service?.DurationMin
            }).ToList()
        };

        return Ok(new { success = true, data = result });
    }

    // POST: api/Appointments
    [HttpPost]
    public async Task<ActionResult> CreateAppointment(CreateAppointmentDto dto)
    {
        // Verifica che il tenant esista
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
        {
            return BadRequest(new { success = false, error = "Tenant non trovato" });
        }

        var appointment = new Appointment
        {
            TenantId = dto.TenantId,
            CustomerId = dto.CustomerId,
            StaffId = dto.StaffId,
            ResourceId = dto.ResourceId,
            StatusId = dto.StatusId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Note = dto.Note,
            IsActive = true,
            CreationTime = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, new
        {
            success = true,
            data = new AppointmentListDto
            {
                Id = appointment.Id,
                TenantId = appointment.TenantId,
                TenantName = tenant.Name,
                StatusId = appointment.StatusId,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Note = appointment.Note,
                IsActive = appointment.IsActive ?? false
            }
        });
    }

    // PUT: api/Appointments/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null || appointment.IsDeleted == true)
        {
            return NotFound(new { success = false, error = "Appuntamento non trovato" });
        }

        appointment.CustomerId = dto.CustomerId;
        appointment.StaffId = dto.StaffId;
        appointment.ResourceId = dto.ResourceId;
        appointment.StatusId = dto.StatusId;
        appointment.StartTime = dto.StartTime;
        appointment.EndTime = dto.EndTime;
        appointment.Note = dto.Note;
        appointment.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Appuntamento aggiornato con successo" });
    }

    // DELETE: api/Appointments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound(new { success = false, error = "Appuntamento non trovato" });
        }

        // Soft delete
        appointment.IsDeleted = true;
        appointment.DeletionTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Appuntamento eliminato con successo" });
    }

    // PUT: api/Appointments/{id}/change-status
    [HttpPut("{id}/change-status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromQuery] int statusId)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null || appointment.IsDeleted == true)
        {
            return NotFound(new { success = false, error = "Appuntamento non trovato" });
        }

        var status = await _context.AppointmentStatuses.FindAsync(statusId);
        if (status == null)
        {
            return BadRequest(new { success = false, error = "Stato non valido" });
        }

        appointment.StatusId = statusId;
        appointment.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = $"Stato cambiato in '{status.Name}'" });
    }

    // GET: api/Appointments/statuses
    [HttpGet("statuses")]
    public async Task<ActionResult> GetStatuses()
    {
        var statuses = await _context.AppointmentStatuses
            .Where(s => s.IsActive == true)
            .Select(s => new AppointmentStatusDto
            {
                Id = s.Id,
                Name = s.Name,
                IsActive = s.IsActive ?? false
            })
            .ToListAsync();

        return Ok(new { success = true, data = statuses });
    }

    // GET: api/Appointments/today
    [HttpGet("today")]
    public async Task<ActionResult> GetTodayAppointments([FromQuery] Guid? tenantId = null)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var query = _context.Appointments
            .Where(a => a.IsDeleted != true && a.StartTime >= today && a.StartTime < tomorrow)
            .Include(a => a.Tenant)
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Status)
            .Include(a => a.AppointmentServices)
                .ThenInclude(asvc => asvc.Service)
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(a => a.TenantId == tenantId.Value);
        }

        var appointments = await query
            .OrderBy(a => a.StartTime)
            .Select(a => new AppointmentListDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                TenantName = a.Tenant.Name,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer != null ? a.Customer.FullName : null,
                StaffId = a.StaffId,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                ServiceId = a.AppointmentServices.FirstOrDefault() != null ? a.AppointmentServices.FirstOrDefault()!.ServiceId : null,
                ServiceName = a.AppointmentServices.FirstOrDefault() != null && a.AppointmentServices.FirstOrDefault()!.Service != null 
                    ? a.AppointmentServices.FirstOrDefault()!.Service!.Name : null,
                TotalPrice = a.AppointmentServices.Sum(s => s.Service != null ? s.Service.BasePrice : 0),
                StatusId = a.StatusId,
                StatusName = a.Status != null ? a.Status.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note,
                IsActive = a.IsActive ?? false
            })
            .ToListAsync();

        return Ok(new { success = true, data = appointments, count = appointments.Count });
    }

    // GET: api/Appointments/week
    [HttpGet("week")]
    public async Task<ActionResult> GetWeekAppointments([FromQuery] Guid? tenantId = null)
    {
        var today = DateTime.Today;
        var endOfWeek = today.AddDays(7);

        var query = _context.Appointments
            .Where(a => a.IsDeleted != true && a.StartTime >= today && a.StartTime < endOfWeek)
            .Include(a => a.Tenant)
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Status)
            .Include(a => a.AppointmentServices)
                .ThenInclude(asvc => asvc.Service)
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(a => a.TenantId == tenantId.Value);
        }

        var appointments = await query
            .OrderBy(a => a.StartTime)
            .Select(a => new AppointmentListDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                TenantName = a.Tenant.Name,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer != null ? a.Customer.FullName : null,
                StaffId = a.StaffId,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                ServiceId = a.AppointmentServices.FirstOrDefault() != null ? a.AppointmentServices.FirstOrDefault()!.ServiceId : null,
                ServiceName = a.AppointmentServices.FirstOrDefault() != null && a.AppointmentServices.FirstOrDefault()!.Service != null 
                    ? a.AppointmentServices.FirstOrDefault()!.Service!.Name : null,
                TotalPrice = a.AppointmentServices.Sum(s => s.Service != null ? s.Service.BasePrice : 0),
                StatusId = a.StatusId,
                StatusName = a.Status != null ? a.Status.Name : null,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Note = a.Note,
                IsActive = a.IsActive ?? false
            })
            .ToListAsync();

        return Ok(new { success = true, data = appointments, count = appointments.Count });
    }
}