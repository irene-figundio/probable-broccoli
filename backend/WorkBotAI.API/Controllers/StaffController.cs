using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StaffController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public StaffController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Staff
    [HttpGet]
    public async Task<ActionResult> GetStaff([FromQuery] Guid? tenantId, [FromQuery] string? search, [FromQuery] int? jobTypeId)
    {
        var query = _context.Staff
            .Where(s => s.IsDeleted != true)
            .Include(s => s.Tenant)
            .Include(s => s.JobType)
            .AsQueryable();

        // Filtro per tenant
        if (tenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == tenantId.Value);
        }

        // Filtro per job type
        if (jobTypeId.HasValue)
        {
            query = query.Where(s => s.JobTypeId == jobTypeId.Value);
        }

        // Filtro ricerca
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s => s.Name.Contains(search));
        }

        var staffRaw = await query
            .OrderBy(s => s.Name)
            .ToListAsync();

        var staffList = new List<StaffListDto>();
        foreach (var s in staffRaw)
        {
            var appointmentsCount = await _context.Appointments
                .CountAsync(a => a.StaffId == s.Id && a.IsDeleted != true);
            
            staffList.Add(new StaffListDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                TenantName = s.Tenant?.Name,
                Name = s.Name,
                JobTypeId = s.JobTypeId,
                JobTypeName = s.JobType?.Name,
                IsActive = s.IsActive ?? true,
                CreationTime = s.CreationTime,
                AppointmentsCount = appointmentsCount
            });
        }

        return Ok(new { success = true, data = staffList, count = staffList.Count });
    }

    // GET: api/Staff/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetStaffMember(int id)
    {
        var staff = await _context.Staff
            .Where(s => s.Id == id && s.IsDeleted != true)
            .Include(s => s.Tenant)
            .Include(s => s.JobType)
            .FirstOrDefaultAsync();

        if (staff == null)
        {
            return NotFound(new { success = false, error = "Membro staff non trovato" });
        }

        // Ultimi appuntamenti
        var recentAppointments = await _context.Appointments
            .Where(a => a.StaffId == id && a.IsDeleted != true)
            .Include(a => a.Customer)
            .Include(a => a.Status)
            .OrderByDescending(a => a.StartTime)
            .Take(10)
            .Select(a => new StaffAppointmentDto
            {
                Id = a.Id,
                StartTime = a.StartTime,
                CustomerName = a.Customer != null ? a.Customer.FullName : null,
                StatusName = a.Status != null ? a.Status.Name : null,
                Note = a.Note
            })
            .ToListAsync();

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
            RecentAppointments = recentAppointments
        };

        return Ok(new { success = true, data = dto });
    }

    // POST: api/Staff
    [HttpPost]
    public async Task<ActionResult> CreateStaff([FromBody] CreateStaffDto dto)
    {
        // Verifica tenant
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
        {
            return BadRequest(new { success = false, error = "Tenant non trovato" });
        }

        var staff = new Staff
        {
            TenantId = dto.TenantId,
            Name = dto.Name,
            JobTypeId = dto.JobTypeId,
            IsActive = true,
            CreationTime = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStaffMember), new { id = staff.Id }, new { 
            success = true, 
            data = new StaffListDto
            {
                Id = staff.Id,
                TenantId = staff.TenantId,
                TenantName = tenant.Name,
                Name = staff.Name,
                JobTypeId = staff.JobTypeId,
                JobTypeName = null,
                IsActive = true,
                CreationTime = staff.CreationTime,
                AppointmentsCount = 0
            }
        });
    }

    // PUT: api/Staff/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDto dto)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff == null || staff.IsDeleted == true)
        {
            return NotFound(new { success = false, error = "Membro staff non trovato" });
        }

        staff.Name = dto.Name;
        staff.JobTypeId = dto.JobTypeId;
        staff.IsActive = dto.IsActive;
        staff.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Membro staff aggiornato" });
    }

    // DELETE: api/Staff/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStaff(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff == null)
        {
            return NotFound(new { success = false, error = "Membro staff non trovato" });
        }

        // Soft delete
        staff.IsDeleted = true;
        staff.DeletionTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Membro staff eliminato" });
    }

    // PUT: api/Staff/{id}/toggle-status
    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStaffStatus(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff == null || staff.IsDeleted == true)
        {
            return NotFound(new { success = false, error = "Membro staff non trovato" });
        }

        staff.IsActive = !(staff.IsActive ?? true);
        staff.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = staff.IsActive == true ? "Membro attivato" : "Membro disattivato", isActive = staff.IsActive });
    }

    // GET: api/Staff/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantStaffStats(Guid tenantId)
    {
        var totalStaff = await _context.Staff
            .Where(s => s.TenantId == tenantId && s.IsDeleted != true)
            .CountAsync();

        var activeStaff = await _context.Staff
            .Where(s => s.TenantId == tenantId && s.IsDeleted != true && s.IsActive == true)
            .CountAsync();

        return Ok(new { 
            success = true, 
            data = new {
                totalStaff,
                activeStaff
            }
        });
    }

    // GET: api/Staff/job-types
    [HttpGet("job-types")]
    public async Task<ActionResult> GetJobTypes()
    {
        var jobTypes = await _context.JobTypes
            .Select(j => new { j.Id, j.Name })
            .OrderBy(j => j.Name)
            .ToListAsync();

        return Ok(new { success = true, data = jobTypes });
    }
}