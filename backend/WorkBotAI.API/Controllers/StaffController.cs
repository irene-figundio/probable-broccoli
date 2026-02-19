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
public class StaffController : ControllerBase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IAuditService _auditService;

    public StaffController(IStaffRepository staffRepository, IAuditService auditService)
    {
        _staffRepository = staffRepository;
        _auditService = auditService;
    }

    // GET: api/Staff
    //[HttpGet]
    //public async Task<ActionResult> GetStaff([FromQuery] Guid? tenantId, [FromQuery] string? search, [FromQuery] int? jobTypeId)
    //{
    //    try
    //    {
    //        var staff = await _staffRepository.GetStaffAsync(tenantId, search, jobTypeId);
    //        if (staff == null) { 
    //            await _auditService.LogActionAsync("Staff", "GetStaff", "No staff members found", null, tenantId);
    //            return Ok(new { success = true, data = Array.Empty<StaffListDto>(), count = 0 });
    //        } else {
    //            await _auditService.LogActionAsync("Staff", "GetStaff", $"Retrieved {staff.Count()} staff members", null, tenantId);
    //        }
    //        return Ok(new { success = true, data = staff, count = staff.Count() });
    //    }
    //    catch (Exception ex)
    //    {
    //        await _auditService.LogErrorAsync("Staff", "Error retrieving staff", ex, tenantId);
    //        return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
    //    }
    //}

    // GET: api/Staff/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetStaffMember(int id)
    {
        try
        {
            var staff = await _staffRepository.GetStaffByIdAsync(id);
            if (staff == null)
            {
                await _auditService.LogErrorAsync("Staff - GetStaffMember", $"Staff member {id} not found", null, staff?.TenantId);
                return NotFound(new { success = false, error = "Membro staff non trovato" });
            }
                

            var recentAppointments = await _staffRepository.GetRecentAppointmentsAsync(id);
            var dto = new StaffDetailDto
            {
                Id = staff.Id,
                TenantId = staff.TenantId,
                TenantName = staff.Tenant?.Name,
                Name = staff.Name,
                JobTypeId = staff.JobTypeId,
                JobTypeName = staff.JobType?.Name,
                IsActive = staff.IsActive ?? true,
                CreationTime = staff.CreationTime,
                IsDeleted = staff.IsDeleted ?? false,
                RecentAppointments = recentAppointments.Select(a => new StaffAppointmentDto
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    CustomerName = a.Customer != null ? a.Customer.FullName : null,
                    StatusName = a.Status != null ? a.Status.Name : null,
                    Note = a.Note
                }).ToList()
            };
            return Ok(new { success = true, data = dto });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Staff", $"Error retrieving staff member {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/Staff
    [HttpPost]
    public async Task<ActionResult> CreateStaff([FromBody] CreateStaffDto dto)
    {
        try
        {
            var staff = new Staff
            {
                TenantId = dto.TenantId,
                Name = dto.Name,
                JobTypeId = dto.JobTypeId,
                IsActive = true,
                CreationTime = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdStaff = await _staffRepository.CreateStaffAsync(staff);
            await _auditService.LogActionAsync("Staff", "Create", $"Created staff member {staff.Name} ({staff.Id})", null, dto.TenantId);

            return CreatedAtAction(nameof(GetStaffMember), new { id = createdStaff.Id }, new
            {
                success = true,
                data = new StaffListDto
                {
                    Id = createdStaff.Id,
                    TenantId = createdStaff.TenantId,
                    Name = createdStaff.Name,
                    JobTypeId = createdStaff.JobTypeId,
                    IsActive = true,
                    CreationTime = createdStaff.CreationTime,
                    AppointmentsCount = 0
                }
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Staff", "Error creating staff member", ex, dto.TenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Staff/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDto dto)
    {
        try
        {
            var staff = await _staffRepository.GetStaffByIdAsync(id);
            if (staff == null)
            {
                await _auditService.LogErrorAsync("Staff - UpdateStaff", $"Staff member {id} not found", null, null);
                return NotFound(new { success = false, error = "Membro staff non trovato" });
            } else {
                await _auditService.LogActionAsync("Staff", "Update", $"Updating staff member {id}", null, staff.TenantId);
            }

            staff.Name = dto.Name;
            staff.JobTypeId = dto.JobTypeId;
            staff.IsActive = dto.IsActive;
            staff.LastModificationTime = DateTime.UtcNow;

            await _staffRepository.UpdateStaffAsync(staff);
            await _auditService.LogActionAsync("Staff", "Update", $"Updated staff member {id}", null, staff.TenantId);

            return Ok(new { success = true, message = "Membro staff aggiornato" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Staff", $"Error updating staff member {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/Staff/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStaff(int id)
    {
        try
        {
            var staff = await _staffRepository.GetStaffByIdAsync(id);
            if (staff == null)
            {
                await _auditService.LogErrorAsync("Staff - DeleteStaff", $"Staff member {id} not found", null, null);
                return NotFound(new { success = false, error = "Membro staff non trovato" });
            }
            await _staffRepository.DeleteStaffAsync(id);
            await _auditService.LogActionAsync("Staff", "Delete", $"Deleted staff member {id}", null, staff.TenantId);
            return Ok(new { success = true, message = "Membro staff eliminato" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Staff", $"Error deleting staff member {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Staff/{id}/toggle-status
    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStaffStatus(int id)
    {
        try
        {
            await _staffRepository.ToggleStaffStatusAsync(id);
            if (id <= 0)
            {
                await _auditService.LogErrorAsync("Staff - ToggleStaffStatus", $"Invalid staff member ID {id}", null, null);
                return BadRequest(new { success = false, error = "ID membro staff non valido" });
            }
            var staff = await _staffRepository.GetStaffByIdAsync(id);
            if (staff == null)
            {
                await _auditService.LogErrorAsync("Staff - ToggleStaffStatus", $"Staff member {id} not found", null, null);
                return NotFound(new { success = false, error = "Membro staff non trovato" });
            }
            await _auditService.LogActionAsync("Staff", "ToggleStatus", $"Toggled status for staff member {id} to {staff?.IsActive}", null, staff?.TenantId);
            return Ok(new { success = true, message = staff?.IsActive == true ? "Membro attivato" : "Membro disattivato", isActive = staff?.IsActive });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Staff", $"Error toggling status for staff member {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Staff/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantStaffStats(Guid tenantId)
    {
        try
        {
            var totalStaff = await _staffRepository.GetTotalStaffAsync(tenantId);
            var activeStaff = await _staffRepository.GetActiveStaffAsync(tenantId);
            return Ok(new { success = true, data = new { totalStaff, activeStaff } });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Staff", $"Error retrieving staff stats for tenant {tenantId}", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Staff/job-types
    [HttpGet("job-types")]
    public async Task<ActionResult> GetJobTypes()
    {
        try
        {
            var jobTypes = await _staffRepository.GetJobTypesAsync();
            if (jobTypes == null)
                {
                await _auditService.LogActionAsync("Staff", "GetJobTypes", "No job types found");
                return Ok(new { success = true, data = Array.Empty<object>() });
            }
            return Ok(new { success = true, data = jobTypes.Select(j => new { j.Id, j.Name }) });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Staff", "Error retrieving job types", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    [HttpGet]
    public async Task<ActionResult> GetStaff(
    [FromQuery] Guid? tenantId,
    [FromQuery] string? search,
    [FromQuery] int? jobTypeId,
    [FromQuery] string? jobTypeGender)
    {
        try
        {
            IEnumerable<StaffListDto> staff;

            try
            {
                staff = await _staffRepository.GetStaffAsync(tenantId, search, jobTypeId, jobTypeGender);
            }
            catch (ArgumentException ex)
            {
                await _auditService.LogErrorAsync("Staff", "Invalid jobTypeGender filter", ex, tenantId);
                return BadRequest(new { success = false, error = ex.Message });
            }

            if (staff == null || !staff.Any())
            {
                await _auditService.LogActionAsync("Staff", "GetStaff", "No staff members found", null, tenantId);
                return Ok(new { success = true, data = Array.Empty<StaffListDto>(), count = 0 });
            }

            await _auditService.LogActionAsync("Staff", "GetStaff", $"Retrieved {staff.Count()} staff members", null, tenantId);
            return Ok(new { success = true, data = staff, count = staff.Count() });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Staff", "Error retrieving staff", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

}
