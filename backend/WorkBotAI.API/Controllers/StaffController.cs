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
public class StaffController : ControllerBase
{
    private readonly IStaffRepository _staffRepository;

    public StaffController(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    // GET: api/Staff
    [HttpGet]
    public async Task<ActionResult> GetStaff([FromQuery] Guid? tenantId, [FromQuery] string? search, [FromQuery] int? jobTypeId)
    {
        var staff = await _staffRepository.GetStaffAsync(tenantId, search, jobTypeId);
        return Ok(new { success = true, data = staff, count = staff.Count() });
    }

    // GET: api/Staff/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetStaffMember(int id)
    {
        var staff = await _staffRepository.GetStaffByIdAsync(id);
        if (staff == null)
            return NotFound(new { success = false, error = "Membro staff non trovato" });

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

    // POST: api/Staff
    [HttpPost]
    public async Task<ActionResult> CreateStaff([FromBody] CreateStaffDto dto)
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

    // PUT: api/Staff/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDto dto)
    {
        var staff = await _staffRepository.GetStaffByIdAsync(id);
        if (staff == null)
            return NotFound(new { success = false, error = "Membro staff non trovato" });

        staff.Name = dto.Name;
        staff.JobTypeId = dto.JobTypeId;
        staff.IsActive = dto.IsActive;
        staff.LastModificationTime = DateTime.UtcNow;

        await _staffRepository.UpdateStaffAsync(staff);
        return Ok(new { success = true, message = "Membro staff aggiornato" });
    }

    // DELETE: api/Staff/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStaff(int id)
    {
        await _staffRepository.DeleteStaffAsync(id);
        return Ok(new { success = true, message = "Membro staff eliminato" });
    }

    // PUT: api/Staff/{id}/toggle-status
    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStaffStatus(int id)
    {
        await _staffRepository.ToggleStaffStatusAsync(id);
        var staff = await _staffRepository.GetStaffByIdAsync(id);
        return Ok(new { success = true, message = staff.IsActive == true ? "Membro attivato" : "Membro disattivato", isActive = staff.IsActive });
    }

    // GET: api/Staff/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantStaffStats(Guid tenantId)
    {
        var totalStaff = await _staffRepository.GetTotalStaffAsync(tenantId);
        var activeStaff = await _staffRepository.GetActiveStaffAsync(tenantId);
        return Ok(new { success = true, data = new { totalStaff, activeStaff } });
    }

    // GET: api/Staff/job-types
    [HttpGet("job-types")]
    public async Task<ActionResult> GetJobTypes()
    {
        var jobTypes = await _staffRepository.GetJobTypesAsync();
        return Ok(new { success = true, data = jobTypes.Select(j => new { j.Id, j.Name }) });
    }
}