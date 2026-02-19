using WorkBotAI.API.Services;
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
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAuditService _auditService;

    public AppointmentsController(IAppointmentRepository appointmentRepository, IAuditService auditService)
    {
        _appointmentRepository = appointmentRepository;
        _auditService = auditService;
    }

    // GET: api/Appointments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentListDto>>> GetAppointments(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] int? statusId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            await _auditService.LogErrorAsync("Appointments - GetAppointments", "Invalid date range", null, null, null);
            return BadRequest(new { success = false, error = "Invalid date range" });
        }

        var appointments = await _appointmentRepository.GetAppointmentsAsync(tenantId, statusId, fromDate, toDate);
        if (appointments == null || !appointments.Any())
        {
            await _auditService.LogActionAsync("Appointments", "GetAppointments", "No appointments found", null, tenantId);
            return Ok(new { success = true, data = new List<AppointmentListDto>() });
        }

        var appointmentDtos = appointments.Select(a => new AppointmentListDto
        {
            Id = a.Id,
            TenantId = a.TenantId,
            TenantName = a.Tenant.Name,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer != null ? a.Customer.FullName : null,
            StaffId = a.StaffId,
            StaffName = a.Staff != null ? a.Staff.Name : null,
            ServiceId = a.AppointmentServices.FirstOrDefault()?.ServiceId,
            ServiceName = a.AppointmentServices.FirstOrDefault()?.Service?.Name,
            TotalPrice = a.AppointmentServices.Sum(s => s.Service != null ? s.Service.BasePrice : 0),
            StatusId = a.StatusId,
            StatusName = a.Status != null ? a.Status.Name : null,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Note = a.Note,
            IsActive = a.IsActive ?? false
        });

        return Ok(new { success = true, data = appointmentDtos });
    }

    // GET: api/Appointments/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetAppointment(int id)
    {
        var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

        if (appointment == null)
        {
            await _auditService.LogErrorAsync("Appointments - GetAppointment", $"Appointment not found: {id}", null, null, null);
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
        try
        {
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

            var createdAppointment = await _appointmentRepository.CreateAppointmentAsync(appointment);
            await _auditService.LogActionAsync("Appointments", "Create", $"Created appointment {createdAppointment.Id} for customer {dto.CustomerId}", null, dto.TenantId);

            return CreatedAtAction(nameof(GetAppointment), new { id = createdAppointment.Id }, new
            {
                success = true,
                data = new AppointmentListDto
                {
                    Id = createdAppointment.Id,
                    TenantId = createdAppointment.TenantId,
                    StatusId = createdAppointment.StatusId,
                    StartTime = createdAppointment.StartTime,
                    EndTime = createdAppointment.EndTime,
                    Note = createdAppointment.Note,
                    IsActive = createdAppointment.IsActive ?? false
                }
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Appointments", "Error creating appointment", ex, dto.TenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Appointments/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);

        if (appointment == null)
        {
            await _auditService.LogErrorAsync("Appointments - UpdateAppointment", $"Appointment not found: {id}", null, null, null);
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

        await _appointmentRepository.UpdateAppointmentAsync(appointment);

        return Ok(new { success = true, message = "Appuntamento aggiornato con successo" });
    }

    // DELETE: api/Appointments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        try
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                await _auditService.LogErrorAsync("Appointments - DeleteAppointment", $"Appointment not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Appuntamento non trovato" });
            }

            await _appointmentRepository.DeleteAppointmentAsync(id);
            await _auditService.LogActionAsync("Appointments", "Delete", $"Deleted appointment {id}");
            return Ok(new { success = true, message = "Appuntamento eliminato con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Appointments", $"Error deleting appointment {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Appointments/{id}/change-status
    [HttpPut("{id}/change-status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromQuery] int statusId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                await _auditService.LogErrorAsync("Appointments - ChangeStatus", $"Appointment not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Appuntamento non trovato" });
            }

            await _appointmentRepository.ChangeAppointmentStatusAsync(id, statusId);
            await _auditService.LogActionAsync("Appointments", "ChangeStatus", $"Changed appointment {id} status to {statusId}");
            return Ok(new { success = true, message = "Stato cambiato con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Appointments", $"Error changing status for appointment {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Appointments/statuses
    [HttpGet("statuses")]
    public async Task<ActionResult> GetStatuses()
    {
        var statuses = await _appointmentRepository.GetAppointmentStatusesAsync();
        if (statuses == null || !statuses.Any())
        {
            await _auditService.LogActionAsync("Appointments", "GetStatuses", "No appointment statuses found");
            return Ok(new { success = true, data = new List<AppointmentStatusDto>() });
        }
        var statusDtos = statuses.Select(s => new AppointmentStatusDto
        {
            Id = s.Id,
            Name = s.Name,
            IsActive = s.IsActive ?? false
        });
        return Ok(new { success = true, data = statusDtos });
    }

    // GET: api/Appointments/today
    [HttpGet("today")]
    public async Task<ActionResult> GetTodayAppointments([FromQuery] Guid? tenantId = null)
    {

        var appointments = await _appointmentRepository.GetTodayAppointmentsAsync(tenantId);
        if (appointments == null || appointments.Count() == 0)
        {
            await _auditService.LogActionAsync("Appointments", "GetTodayAppointments", "No appointments found for today", null, tenantId);
            return Ok(new { success = true, data = new List<AppointmentListDto>() });
        }
        var appointmentDtos = appointments.Select(a => new AppointmentListDto
        {
            Id = a.Id,
            TenantId = a.TenantId,
            TenantName = a.Tenant.Name,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer != null ? a.Customer.FullName : null,
            StaffId = a.StaffId,
            StaffName = a.Staff != null ? a.Staff.Name : null,
            ServiceId = a.AppointmentServices.FirstOrDefault()?.ServiceId,
            ServiceName = a.AppointmentServices.FirstOrDefault()?.Service?.Name,
            TotalPrice = a.AppointmentServices.Sum(s => s.Service != null ? s.Service.BasePrice : 0),
            StatusId = a.StatusId,
            StatusName = a.Status != null ? a.Status.Name : null,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Note = a.Note,
            IsActive = a.IsActive ?? false
        });

        return Ok(new { success = true, data = appointmentDtos, count = appointmentDtos.Count() });
    }

    // GET: api/Appointments/week
    [HttpGet("week")]
    public async Task<ActionResult> GetWeekAppointments([FromQuery] Guid? tenantId = null)
    {
        var appointments = await _appointmentRepository.GetWeekAppointmentsAsync(tenantId);
        if (appointments == null || appointments.Count() == 0)
        {
            await _auditService.LogActionAsync("Appointments", "GetWeekAppointments", "No appointments found for this week", null, tenantId);
            return Ok(new { success = true, data = new List<AppointmentListDto>() });
        }
        var appointmentDtos = appointments.Select(a => new AppointmentListDto
        {
            Id = a.Id,
            TenantId = a.TenantId,
            TenantName = a.Tenant.Name,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer != null ? a.Customer.FullName : null,
            StaffId = a.StaffId,
            StaffName = a.Staff != null ? a.Staff.Name : null,
            ServiceId = a.AppointmentServices.FirstOrDefault()?.ServiceId,
            ServiceName = a.AppointmentServices.FirstOrDefault()?.Service?.Name,
            TotalPrice = a.AppointmentServices.Sum(s => s.Service != null ? s.Service.BasePrice : 0),
            StatusId = a.StatusId,
            StatusName = a.Status != null ? a.Status.Name : null,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Note = a.Note,
            IsActive = a.IsActive ?? false
        });

        return Ok(new { success = true, data = appointmentDtos, count = appointmentDtos.Count() });
    }
}