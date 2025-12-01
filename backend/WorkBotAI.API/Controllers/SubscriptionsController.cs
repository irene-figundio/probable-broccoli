using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public SubscriptionsController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Subscriptions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubscriptionListDto>>> GetSubscriptions([FromQuery] Guid? tenantId = null, [FromQuery] int? statusId = null)
    {
        var query = _context.Subscriptions
            .Include(s => s.Tenant)
            .Include(s => s.Plane)
            .Include(s => s.Status)
            .AsQueryable();

        // Filtro per tenant
        if (tenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == tenantId.Value);
        }

        // Filtro per stato
        if (statusId.HasValue)
        {
            query = query.Where(s => s.StatusId == statusId.Value);
        }

        var subscriptions = await query
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SubscriptionListDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                TenantName = s.Tenant.Name,
                TenantAcronym = s.Tenant.Acronym,
                PlaneId = s.PlaneId,
                PlaneName = s.Plane != null ? s.Plane.Name : null,
                StatusId = s.StatusId,
                StatusName = s.Status != null ? s.Status.Name : null,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            })
            .ToListAsync();

        return Ok(new { success = true, data = subscriptions });
    }

    // GET: api/Subscriptions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriptionDetailDto>> GetSubscription(int id)
    {
        var subscription = await _context.Subscriptions
            .Where(s => s.Id == id)
            .Include(s => s.Tenant)
            .Include(s => s.Plane)
            .Include(s => s.Status)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync();

        if (subscription == null)
        {
            return NotFound(new { success = false, error = "Abbonamento non trovato" });
        }

        var result = new SubscriptionDetailDto
        {
            Id = subscription.Id,
            TenantId = subscription.TenantId,
            TenantName = subscription.Tenant.Name,
            TenantAcronym = subscription.Tenant.Acronym,
            PlaneId = subscription.PlaneId,
            PlaneName = subscription.Plane?.Name,
            PlaneDescription = subscription.Plane?.Description,
            StatusId = subscription.StatusId,
            StatusName = subscription.Status?.Name,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            TotalPayments = subscription.Payments.Count,
            TotalPaid = subscription.Payments.Sum(p => p.ImportValue ?? 0)
        };

        return Ok(new { success = true, data = result });
    }

    // POST: api/Subscriptions
    [HttpPost]
    public async Task<ActionResult> CreateSubscription(CreateSubscriptionDto dto)
    {
        // Verifica che il tenant esista
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
        {
            return BadRequest(new { success = false, error = "Tenant non trovato" });
        }

        // Verifica che il piano esista
        var plane = await _context.Planes.FindAsync(dto.PlaneId);
        if (plane == null)
        {
            return BadRequest(new { success = false, error = "Piano non trovato" });
        }

        // Disattiva eventuali abbonamenti attivi precedenti
        var activeSubscriptions = await _context.Subscriptions
            .Where(s => s.TenantId == dto.TenantId && s.Status != null && s.Status.Name == "active")
            .ToListAsync();

        foreach (var sub in activeSubscriptions)
        {
            sub.StatusId = 3; // expired
        }

        var subscription = new Subscription
        {
            TenantId = dto.TenantId,
            PlaneId = dto.PlaneId,
            StatusId = dto.StatusId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, new
        {
            success = true,
            data = new SubscriptionListDto
            {
                Id = subscription.Id,
                TenantId = subscription.TenantId,
                TenantName = tenant.Name,
                PlaneId = subscription.PlaneId,
                PlaneName = plane.Name,
                StatusId = subscription.StatusId,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate
            }
        });
    }

    // PUT: api/Subscriptions/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubscription(int id, UpdateSubscriptionDto dto)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);

        if (subscription == null)
        {
            return NotFound(new { success = false, error = "Abbonamento non trovato" });
        }

        subscription.PlaneId = dto.PlaneId;
        subscription.StatusId = dto.StatusId;
        subscription.StartDate = dto.StartDate;
        subscription.EndDate = dto.EndDate;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Abbonamento aggiornato con successo" });
    }

    // DELETE: api/Subscriptions/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);

        if (subscription == null)
        {
            return NotFound(new { success = false, error = "Abbonamento non trovato" });
        }

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Abbonamento eliminato con successo" });
    }

    // PUT: api/Subscriptions/{id}/change-status
    [HttpPut("{id}/change-status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromQuery] int statusId)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);

        if (subscription == null)
        {
            return NotFound(new { success = false, error = "Abbonamento non trovato" });
        }

        var status = await _context.SubscriptionStatuses.FindAsync(statusId);
        if (status == null)
        {
            return BadRequest(new { success = false, error = "Stato non valido" });
        }

        subscription.StatusId = statusId;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = $"Stato cambiato in '{status.Name}'" });
    }

    // PUT: api/Subscriptions/{id}/renew
    [HttpPut("{id}/renew")]
    public async Task<IActionResult> RenewSubscription(int id, [FromQuery] int months = 12)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);

        if (subscription == null)
        {
            return NotFound(new { success = false, error = "Abbonamento non trovato" });
        }

        // Calcola nuova data di scadenza
        var currentEnd = subscription.EndDate ?? DateOnly.FromDateTime(DateTime.Today);
        var newEnd = currentEnd < DateOnly.FromDateTime(DateTime.Today) 
            ? DateOnly.FromDateTime(DateTime.Today.AddMonths(months))
            : currentEnd.AddMonths(months);

        subscription.EndDate = newEnd;
        subscription.StatusId = 1; // active

        await _context.SaveChangesAsync();

        return Ok(new { 
            success = true, 
            message = $"Abbonamento rinnovato fino al {newEnd:dd/MM/yyyy}",
            newEndDate = newEnd
        });
    }

    // GET: api/Subscriptions/planes
    [HttpGet("planes")]
    public async Task<ActionResult> GetPlanes()
    {
        var planes = await _context.Planes
            .Where(p => p.IsActive == true)
            .Select(p => new PlaneDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive ?? false
            })
            .ToListAsync();

        return Ok(new { success = true, data = planes });
    }

    // GET: api/Subscriptions/statuses
    [HttpGet("statuses")]
    public async Task<ActionResult> GetStatuses()
    {
        var statuses = await _context.SubscriptionStatuses
            .Where(s => s.IsActive == true)
            .Select(s => new SubscriptionStatusDto
            {
                Id = s.Id,
                Name = s.Name,
                IsActive = s.IsActive ?? false
            })
            .ToListAsync();

        return Ok(new { success = true, data = statuses });
    }

    // GET: api/Subscriptions/expiring
    [HttpGet("expiring")]
    public async Task<ActionResult> GetExpiringSubscriptions([FromQuery] int days = 30)
    {
        var limitDate = DateOnly.FromDateTime(DateTime.Today.AddDays(days));

        var subscriptions = await _context.Subscriptions
            .Where(s => s.Status != null && s.Status.Name == "active" && s.EndDate <= limitDate)
            .Include(s => s.Tenant)
            .Include(s => s.Plane)
            .OrderBy(s => s.EndDate)
            .Select(s => new SubscriptionListDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                TenantName = s.Tenant.Name,
                TenantAcronym = s.Tenant.Acronym,
                PlaneId = s.PlaneId,
                PlaneName = s.Plane != null ? s.Plane.Name : null,
                StatusId = s.StatusId,
                StatusName = "active",
                StartDate = s.StartDate,
                EndDate = s.EndDate
            })
            .ToListAsync();

        return Ok(new { success = true, data = subscriptions, count = subscriptions.Count });
    }
}