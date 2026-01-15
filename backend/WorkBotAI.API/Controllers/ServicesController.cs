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
public class ServicesController : ControllerBase
{
    private readonly IServiceRepository _serviceRepository;

    public ServicesController(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    // GET: api/Services
    [HttpGet]
    public async Task<ActionResult> GetServices([FromQuery] Guid? tenantId, [FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var services = await _serviceRepository.GetServicesAsync(tenantId, search, categoryId);
        return Ok(new { success = true, data = services, count = services.Count() });
    }

    // GET: api/Services/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetService(int id)
    {
        var service = await _serviceRepository.GetServiceByIdAsync(id);
        if (service == null)
            return NotFound(new { success = false, error = "Servizio non trovato" });

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

    // PUT: api/Services/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateService(int id, [FromBody] UpdateServiceDto dto)
    {
        var service = await _serviceRepository.GetServiceByIdAsync(id);
        if (service == null)
            return NotFound(new { success = false, error = "Servizio non trovato" });

        service.CategoryId = dto.CategoryId;
        service.Name = dto.Name;
        service.Description = dto.Description;
        service.DurationMin = dto.DurationMin;
        service.BasePrice = dto.BasePrice;
        service.BafferTime = dto.BufferTime;
        service.IsActive = dto.IsActive;
        service.LastModificationTime = DateTime.UtcNow;

        await _serviceRepository.UpdateServiceAsync(service);
        return Ok(new { success = true, message = "Servizio aggiornato" });
    }

    // DELETE: api/Services/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteService(int id)
    {
        await _serviceRepository.DeleteServiceAsync(id);
        return Ok(new { success = true, message = "Servizio eliminato" });
    }

    // PUT: api/Services/{id}/toggle-status
    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult> ToggleServiceStatus(int id)
    {
        await _serviceRepository.ToggleServiceStatusAsync(id);
        var service = await _serviceRepository.GetServiceByIdAsync(id);
        return Ok(new { success = true, message = service.IsActive == true ? "Servizio attivato" : "Servizio disattivato", isActive = service.IsActive });
    }

    // GET: api/Services/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantServiceStats(Guid tenantId)
    {
        var totalServices = await _serviceRepository.GetTotalServicesAsync(tenantId);
        var activeServices = await _serviceRepository.GetActiveServicesAsync(tenantId);
        return Ok(new { success = true, data = new { totalServices, activeServices } });
    }

    // GET: api/Services/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _serviceRepository.GetCategoriesAsync();
        return Ok(new { success = true, data = categories.Select(c => new { c.Id, c.Name }) });
    }
}