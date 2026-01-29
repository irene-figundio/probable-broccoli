using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public TenantsController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Tenants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantListDto>>> GetTenants()
    {
        var tenants = await _context.Tenants
            .Where(t => t.IsDeleted != true)
            .Include(t => t.Category)
            .Include(t => t.Subscriptions)
                .ThenInclude(s => s.Plane)
            .Include(t => t.Subscriptions)
                .ThenInclude(s => s.Status)
            .Include(t => t.Users)
            .Include(t => t.Appointments)
            .OrderByDescending(t => t.CreationDate)
            .Select(t => new TenantListDto
            {
                Id = t.Id,
                Name = t.Name,
                Acronym = t.Acronym,
                LogoImage = t.LogoImage,
                CategoryName = t.Category != null ? t.Category.Name : null,
                IsActive = t.IsActive ?? false,
                CreationDate = t.CreationDate,
                SubscriptionPlan = t.Subscriptions
                    .Where(s => s.Status != null && s.Status.Name == "active")
                    .Select(s => s.Plane != null ? s.Plane.Name : null)
                    .FirstOrDefault(),
                SubscriptionStatus = t.Subscriptions
                    .OrderByDescending(s => s.StartDate)
                    .Select(s => s.Status != null ? s.Status.Name : null)
                    .FirstOrDefault(),
                TotalUsers = t.Users.Count(u => u.IsDeleted != true),
                TotalAppointments = t.Appointments.Count(a => a.IsDeleted != true)
            })
            .ToListAsync();

        return Ok(new { success = true, data = tenants });
    }

    // GET: api/Tenants/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDetailDto>> GetTenant(Guid id)
    {
        var tenant = await _context.Tenants
            .Where(t => t.Id == id && t.IsDeleted != true)
            .Include(t => t.Category)
            .Include(t => t.Subscriptions)
                .ThenInclude(s => s.Plane)
            .Include(t => t.Subscriptions)
                .ThenInclude(s => s.Status)
            .Include(t => t.Users)
            .Include(t => t.Appointments)
            .Include(t => t.Customers)
            .Include(t => t.Services)
            .FirstOrDefaultAsync();

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant non trovato" });
        }

        var activeSubscription = tenant.Subscriptions
            .Where(s => s.Status != null && (s.Status.Name == "active" || s.Status.Name == "trial"))
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault();

        var result = new TenantDetailDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Acronym = tenant.Acronym,
            LogoImage = tenant.LogoImage,
            CategoryId = tenant.CategoryId,
            CategoryName = tenant.Category?.Name,
            IsActive = tenant.IsActive ?? false,
            CreationDate = tenant.CreationDate,
            Subscription = activeSubscription != null ? new SubscriptionInfoDto
            {
                Id = activeSubscription.Id,
                PlanName = activeSubscription.Plane?.Name,
                Status = activeSubscription.Status?.Name,
                StartDate = activeSubscription.StartDate,
                EndDate = activeSubscription.EndDate
            } : null,
            TotalUsers = tenant.Users.Count(u => u.IsDeleted != true),
            TotalAppointments = tenant.Appointments.Count(a => a.IsDeleted != true),
            TotalCustomers = tenant.Customers.Count(c => c.IsDeleted != true),
            TotalServices = tenant.Services.Count(s => s.IsDeleted != true)
        };

        return Ok(new { success = true, data = result });
    }

    // POST: api/Tenants
    [HttpPost]
    public async Task<ActionResult<TenantDetailDto>> CreateTenant(CreateTenantDto dto)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Acronym = dto.Acronym,
            LogoImage = dto.LogoImage,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            IsDeleted = false,
            CreationDate = DateTime.UtcNow
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, new 
        { 
            success = true, 
            data = new TenantListDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Acronym = tenant.Acronym,
                IsActive = tenant.IsActive ?? false,
                CreationDate = tenant.CreationDate
            }
        });
    }

    // PUT: api/Tenants/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenant(Guid id, UpdateTenantDto dto)
    {
        var tenant = await _context.Tenants
            .Where(t => t.Id == id && t.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant non trovato" });
        }

        tenant.Name = dto.Name;
        tenant.Acronym = dto.Acronym;
        tenant.LogoImage = dto.LogoImage;
        tenant.CategoryId = dto.CategoryId;
        tenant.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Tenant aggiornato con successo" });
    }

    // DELETE: api/Tenants/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        var tenant = await _context.Tenants
            .Where(t => t.Id == id && t.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (tenant == null)
        {
            return NotFound(new { success = false, error = "Tenant non trovato" });
        }

        // Soft delete
        tenant.IsDeleted = true;
        tenant.DeletionTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Tenant eliminato con successo" });
    }

    // GET: api/Tenants/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive == true)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        return Ok(new { success = true, data = categories });
    }
}