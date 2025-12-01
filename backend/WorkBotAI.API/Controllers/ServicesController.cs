using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public ServicesController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Services
    [HttpGet]
    public async Task<ActionResult> GetServices([FromQuery] Guid? tenantId, [FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var query = _context.Services
            .Where(s => s.IsDeleted != true)
            .Include(s => s.Tenant)
            .Include(s => s.Category)
            .AsQueryable();

        // Filtro per tenant
        if (tenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == tenantId.Value);
        }

        // Filtro per categoria
        if (categoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == categoryId.Value);
        }

        // Filtro ricerca
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s => 
                (s.Name != null && s.Name.Contains(search)) ||
                (s.Description != null && s.Description.Contains(search))
            );
        }

        var servicesRaw = await query
            .OrderBy(s => s.Name)
            .ToListAsync();

        var services = new List<ServiceListDto>();
        foreach (var s in servicesRaw)
        {
            var appointmentsCount = await _context.AppointmentServices
                .CountAsync(aps => aps.ServiceId == s.Id);
            
            services.Add(new ServiceListDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                TenantName = s.Tenant?.Name,
                Name = s.Name,
                Description = s.Description,
                CategoryName = s.Category?.Name,
                DurationMin = s.DurationMin,
                BasePrice = s.BasePrice,
                IsActive = s.IsActive ?? true,
                CreationTime = s.CreationTime,
                AppointmentsCount = appointmentsCount
            });
        }

        return Ok(new { success = true, data = services, count = services.Count });
    }

    // GET: api/Services/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetService(int id)
    {
        var service = await _context.Services
            .Where(s => s.Id == id && s.IsDeleted != true)
            .Include(s => s.Tenant)
            .Include(s => s.Category)
            .FirstOrDefaultAsync();

        if (service == null)
        {
            return NotFound(new { success = false, error = "Servizio non trovato" });
        }

        var dto = new ServiceDetailDto
        {
            Id = service.Id,
            TenantId = service.TenantId,
            TenantName = service.Tenant?.Name,
            CategoryId = service.CategoryId,
            CategoryName = service.Category?.Name,
            Name = service.Name,
            Description = service.Description,
            DurationMin = service.DurationMin,
            BasePrice = service.BasePrice,
            BufferTime = service.BafferTime,
            IsActive = service.IsActive ?? true,
            CreationTime = service.CreationTime,
            IsDeleted = service.IsDeleted ?? false
        };

        return Ok(new { success = true, data = dto });
    }

    // POST: api/Services
    [HttpPost]
    public async Task<ActionResult> CreateService([FromBody] CreateServiceDto dto)
    {
        // Verifica tenant
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
        {
            return BadRequest(new { success = false, error = "Tenant non trovato" });
        }

        var service = new Service
        {
            TenantId = dto.TenantId,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            DurationMin = dto.DurationMin,
            BasePrice = dto.BasePrice,
            BafferTime = dto.BufferTime,
            IsActive = true,
            CreationTime = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetService), new { id = service.Id }, new { 
            success = true, 
            data = new ServiceListDto
            {
                Id = service.Id,
                TenantId = service.TenantId,
                TenantName = tenant.Name,
                Name = service.Name,
                Description = service.Description,
                CategoryName = null,
                DurationMin = service.DurationMin,
                BasePrice = service.BasePrice,
                IsActive = true,
                CreationTime = service.CreationTime,
                AppointmentsCount = 0
            }
        });
    }

    // PUT: api/Services/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateService(int id, [FromBody] UpdateServiceDto dto)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null || service.IsDeleted == true)
        {
            return NotFound(new { success = false, error = "Servizio non trovato" });
        }

        service.CategoryId = dto.CategoryId;
        service.Name = dto.Name;
        service.Description = dto.Description;
        service.DurationMin = dto.DurationMin;
        service.BasePrice = dto.BasePrice;
        service.BafferTime = dto.BufferTime;
        service.IsActive = dto.IsActive;
        service.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Servizio aggiornato" });
    }

    // DELETE: api/Services/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            return NotFound(new { success = false, error = "Servizio non trovato" });
        }

        // Soft delete
        service.IsDeleted = true;
        service.DeletionTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Servizio eliminato" });
    }

    // PUT: api/Services/{id}/toggle-status
    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult> ToggleServiceStatus(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null || service.IsDeleted == true)
        {
            return NotFound(new { success = false, error = "Servizio non trovato" });
        }

        service.IsActive = !(service.IsActive ?? true);
        service.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = service.IsActive == true ? "Servizio attivato" : "Servizio disattivato", isActive = service.IsActive });
    }

    // GET: api/Services/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantServiceStats(Guid tenantId)
    {
        var totalServices = await _context.Services
            .Where(s => s.TenantId == tenantId && s.IsDeleted != true)
            .CountAsync();

        var activeServices = await _context.Services
            .Where(s => s.TenantId == tenantId && s.IsDeleted != true && s.IsActive == true)
            .CountAsync();

        return Ok(new { 
            success = true, 
            data = new {
                totalServices,
                activeServices
            }
        });
    }

    // GET: api/Services/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new { c.Id, c.Name })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(new { success = true, data = categories });
    }
}