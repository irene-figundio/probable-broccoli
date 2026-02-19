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
public class ServicesController : ControllerBase
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IAuditService _auditService;

    public ServicesController(IServiceRepository serviceRepository, IAuditService auditService)
    {
        _serviceRepository = serviceRepository;
        _auditService = auditService;
    }

    // GET: api/Services
    [HttpGet]
    public async Task<ActionResult> GetServices([FromQuery] Guid? tenantId, [FromQuery] string? search, [FromQuery] int? categoryId)
    {
        try
        {
            var services = await _serviceRepository.GetServicesAsync(tenantId, search, categoryId);
            if (services == null) {
                await _auditService.LogActionAsync("Services", "GetServices", "No services found", null, tenantId);
                return Ok(new { success = true, data = Array.Empty<ServiceListDto>(), count = 0 });
            } else {
                await _auditService.LogActionAsync("Services", "GetServices", $"Retrieved {services.Count()} services", null, tenantId);
            }
            return Ok(new { success = true, data = services, count = services.Count() });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Services", "Error retrieving services", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Services/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetService(int id)
    {
        try
        {
            var service = await _serviceRepository.GetServiceByIdAsync(id);
            if (service == null)
            {   await _auditService.LogActionAsync("Services", "GetService", $"Service {id} not found", null, service?.TenantId);
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
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Services", $"Error retrieving service {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/Services
    [HttpPost]
    public async Task<ActionResult> CreateService([FromBody] CreateServiceDto dto)
    {
        try
        {
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

            var createdService = await _serviceRepository.CreateServiceAsync(service);
            await _auditService.LogActionAsync("Services", "Create", $"Created service {service.Name} ({service.Id})", null, dto.TenantId);

            return CreatedAtAction(nameof(GetService), new { id = createdService.Id }, new
            {
                success = true,
                data = new ServiceListDto
                {
                    Id = createdService.Id,
                    TenantId = createdService.TenantId,
                    Name = createdService.Name,
                    Description = createdService.Description,
                    DurationMin = createdService.DurationMin,
                    BasePrice = createdService.BasePrice,
                    IsActive = true,
                    CreationTime = createdService.CreationTime,
                    AppointmentsCount = 0
                }
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Services", "Error creating service", ex, dto.TenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Services/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateService(int id, [FromBody] UpdateServiceDto dto)
    {
        try
        {
            var service = await _serviceRepository.GetServiceByIdAsync(id);
            if (service == null)
            {       await _auditService.LogActionAsync("Services", "Update", $"Service {id} not found for update", null, service?.TenantId);
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

            await _serviceRepository.UpdateServiceAsync(service);
            await _auditService.LogActionAsync("Services", "Update", $"Updated service {id}", null, service.TenantId);

            return Ok(new { success = true, message = "Servizio aggiornato" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Services", $"Error updating service {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/Services/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteService(int id)
    {
        try
        {
            var service = await _serviceRepository.GetServiceByIdAsync(id);            
            if (service == null)
            {       await _auditService.LogErrorAsync("Services - Delete", $"Service {id} not found for deletion", null, service?.TenantId);
                    return NotFound(new { success = false, error = "Servizio non trovato" });
            }
            await _serviceRepository.DeleteServiceAsync(id);
            await _auditService.LogActionAsync("Services", "Delete", $"Deleted service {id}");
            return Ok(new { success = true, message = "Servizio eliminato" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Services", $"Error deleting service {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Services/{id}/toggle-status
    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult> ToggleServiceStatus(int id)
    {
        try
        {
            await _serviceRepository.ToggleServiceStatusAsync(id);
            var service = await _serviceRepository.GetServiceByIdAsync(id);
            await _auditService.LogActionAsync("Services", "ToggleStatus", $"Toggled status for service {id} to {service?.IsActive}", null, service?.TenantId);
            return Ok(new { success = true, message = service?.IsActive == true ? "Servizio attivato" : "Servizio disattivato", isActive = service?.IsActive });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Services", $"Error toggling status for service {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Services/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantServiceStats(Guid tenantId)
    {
        try
        {
            var totalServices = await _serviceRepository.GetTotalServicesAsync(tenantId);
            var activeServices = await _serviceRepository.GetActiveServicesAsync(tenantId);
            return Ok(new { success = true, data = new { totalServices, activeServices } });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Services", $"Error retrieving service stats for tenant {tenantId}", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Services/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        try
        {
            var categories = await _serviceRepository.GetCategoriesAsync();
            if (categories == null) {
                await _auditService.LogActionAsync("Services", "GetCategories", "No service categories found");
                return Ok(new { success = true, data = Array.Empty<object>() });
            } else {
                await _auditService.LogActionAsync("Services", "GetCategories", $"Retrieved {categories.Count()} service categories");
            }
            return Ok(new { success = true, data = categories.Select(c => new { c.Id, c.Name }) });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Services", "Error retrieving service categories", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
