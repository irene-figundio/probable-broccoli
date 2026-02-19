using WorkBotAI.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkbotAI.Models;
using WorkBotAI.API.Services;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IAuditService _auditService;

    public TenantsController(ITenantRepository tenantRepository, IAuditService auditService)
    {
        _tenantRepository = tenantRepository;
        _auditService = auditService;
    }

    // GET: api/Tenants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantListDto>>> GetTenants()
    {
        try
        {
            var tenants = await _tenantRepository.GetTenantsAsync();
            if (tenants == null || !tenants.Any())
            {
                await _auditService.LogActionAsync("Tenants", "GetTenants", "No tenants found");
                return Ok(new { success = true, data = Array.Empty<TenantListDto>() });
            }
            var dtos = tenants.Select(t => new TenantListDto
            {
                Id = t.Id,
                Name = t.Name,
                Acronym = t.Acronym,
                LogoImage = t.LogoImage,
                CreationDate = t.CreationDate,
                IsActive = t.IsActive ?? false,
                CategoryName = t.Category?.Name
            });

            return Ok(new { success = true, data = dtos });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Tenants", "Error retrieving tenants", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Tenants/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDetailDto>> GetTenant(Guid id)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(id);

            if (tenant == null)
            {
                await _auditService.LogErrorAsync("Tenants - GetTenant", $"Tenant {id} not found");
                return NotFound(new { success = false, error = "Tenant non trovato" });
            }

            var result = new TenantDetailDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Acronym = tenant.Acronym,
                LogoImage = tenant.LogoImage,
                CreationDate = tenant.CreationDate,
                IsActive = tenant.IsActive ?? false,
                CategoryId = tenant.CategoryId,
                CategoryName = tenant.Category?.Name
            };

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Tenants", $"Error retrieving tenant {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/Tenants
    [HttpPost]
    public async Task<ActionResult> CreateTenant(CreateTenantDto dto)
    {
        try
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Acronym = dto.Acronym,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreationDate = DateTime.UtcNow,
                IsDeleted = false
            };

            await _tenantRepository.CreateTenantAsync(tenant);
            await _auditService.LogActionAsync("Tenants", "Create", $"Created tenant {tenant.Name} ({tenant.Id})");

            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, new
            {
                success = true,
                data = new { id = tenant.Id, name = tenant.Name }
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Tenants", "Error creating tenant", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Tenants/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenant(Guid id, UpdateTenantDto dto)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(id);

            if (tenant == null)
            {
                await _auditService.LogErrorAsync("Tenants - UpdateTenant", $"Tenant {id} not found");
                return NotFound(new { success = false, error = "Tenant non trovato" });
            }

            tenant.Name = dto.Name;
            tenant.Acronym = dto.Acronym;
            tenant.CategoryId = dto.CategoryId;
            tenant.IsActive = dto.IsActive;

            await _tenantRepository.UpdateTenantAsync(tenant);
            await _auditService.LogActionAsync("Tenants", "Update", $"Updated tenant {tenant.Name} ({id})");

            return Ok(new { success = true, message = "Tenant aggiornato con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Tenants", $"Error updating tenant {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/Tenants/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        try
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(id);
            if (tenant == null)
            {
                await _auditService.LogErrorAsync("Tenants - DeleteTenant", $"Tenant {id} not found");
                return NotFound(new { success = false, error = "Tenant non trovato" });
            }

            await _tenantRepository.DeleteTenantAsync(id);
            await _auditService.LogActionAsync("Tenants", "Delete", $"Deleted tenant {id}");

            return Ok(new { success = true, message = "Tenant eliminato con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Tenants", $"Error deleting tenant {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Tenants/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        try
        {
            var categories = await _tenantRepository.GetCategoriesAsync();
            if (categories == null || !categories.Any())
            {
                await _auditService.LogActionAsync("Tenants", "GetCategories", "No categories found");
                return Ok(new { success = true, data = Array.Empty<object>() });
            }
            return Ok(new { success = true, data = categories.Select(c => new { c.Id, c.Name }) });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Tenants", "Error retrieving categories", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
