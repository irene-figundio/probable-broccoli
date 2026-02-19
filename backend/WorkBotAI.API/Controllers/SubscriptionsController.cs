using WorkBotAI.API.DTOs;
using WorkBotAI.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(ISubscriptionRepository repository, IAuditService auditService, ILogger<SubscriptionsController> logger)
    {
        _repository = repository;
        _auditService = auditService;
        _logger = logger;
    }

    // GET: api/Subscriptions
    [HttpGet]
    public async Task<ActionResult> GetSubscriptions([FromQuery] Guid? tenantId = null, [FromQuery] int? statusId = null)
    {
        try
        {
            var subscriptions = await _repository.GetSubscriptionsAsync(tenantId, statusId);
            if (subscriptions == null || !subscriptions.Any())
            {
                await _auditService.LogActionAsync("Subscriptions", "Get", $"No subscriptions found for tenantId={tenantId} and statusId={statusId}");
                return Ok(new { success = true, data = Array.Empty<SubscriptionListDto>() });
            }

            var result = subscriptions.Select(s => new SubscriptionListDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                TenantName = s.Tenant.Name,
                TenantAcronym = s.Tenant.Acronym,
                PlaneId = s.PlaneId,
                PlaneName = s.Plane?.Name,
                StatusId = s.StatusId,
                StatusName = s.Status?.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            });

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero degli abbonamenti");
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // GET: api/Subscriptions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetSubscription(int id)
    {
        try
        {
            var subscription = await _repository.GetSubscriptionByIdAsync(id);


            if (subscription == null)
            {
                await _auditService.LogErrorAsync("Subscriptions - Get", $"Subscription {id} not found");
                return NotFound(new { success = false, error = "NOT_FOUND" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero dell'abbonamento {Id}", id);
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // POST: api/Subscriptions
    [HttpPost]
    public async Task<ActionResult> CreateSubscription(CreateSubscriptionDto dto)
    {
        try
        {
            // Validazione input base
            if (dto.TenantId == Guid.Empty) 
            {
                await _auditService.LogErrorAsync("Subscriptions - Create", $"Invalid tenant ID {dto.TenantId}");
                return BadRequest(new { success = false, error = "INVALID_TENANT_ID" });
            }
            if (dto.PlaneId <= 0) 
            { 
                await _auditService.LogErrorAsync("Subscriptions - Create", $"Invalid plane ID {dto.PlaneId} for tenant {dto.TenantId}");
                return BadRequest(new { success = false, error = "INVALID_PLANE_ID" }); 
            }

            // Verifica che il piano esista
            var plane = await _repository.GetPlaneByIdAsync(dto.PlaneId);
            if (plane == null)
            {
                await _auditService.LogErrorAsync("Subscriptions - Create", $"Plane {dto.PlaneId} not found for tenant {dto.TenantId}");
                return BadRequest(new { success = false, error = "PLANE_NOT_FOUND" });
            }

            // Disattiva eventuali abbonamenti attivi precedenti
            await _repository.DeactivateActiveSubscriptionsAsync(dto.TenantId);

            var subscription = new Subscription
            {
                TenantId = dto.TenantId,
                PlaneId = dto.PlaneId,
                StatusId = dto.StatusId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            await _repository.CreateSubscriptionAsync(subscription);
            await _auditService.LogActionAsync("Subscriptions", "Create", $"Created subscription {subscription.Id} for tenant {dto.TenantId}");

            return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, new
            {
                success = true,
                data = new { id = subscription.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nella creazione dell'abbonamento");
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // PUT: api/Subscriptions/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubscription(int id, UpdateSubscriptionDto dto)
    {
        try
        {
            var subscription = await _repository.GetSubscriptionByIdAsync(id);

            if (subscription == null)
            {
                await _auditService.LogErrorAsync("Subscriptions - Update", $"Subscription {id} not found");
                return NotFound(new { success = false, error = "NOT_FOUND" });
            }

            subscription.PlaneId = dto.PlaneId;
            subscription.StatusId = dto.StatusId;
            subscription.StartDate = dto.StartDate;
            subscription.EndDate = dto.EndDate;

            await _repository.UpdateSubscriptionAsync(subscription);
            await _auditService.LogActionAsync("Subscriptions", "Update", $"Updated subscription {id}");

            return Ok(new { success = true, message = "Abbonamento aggiornato con successo" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'aggiornamento dell'abbonamento {Id}", id);
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // DELETE: api/Subscriptions/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        try
        {
            var subscription = await _repository.GetSubscriptionByIdAsync(id);
            if (subscription == null)
            {
                await _auditService.LogErrorAsync("Subscriptions - Delete", $"Subscription {id} not found");
                return NotFound(new { success = false, error = "NOT_FOUND" });
            }

            await _repository.DeleteSubscriptionAsync(id);
            await _auditService.LogActionAsync("Subscriptions", "Delete", $"Deleted subscription {id}");

            return Ok(new { success = true, message = "Abbonamento eliminato con successo" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'eliminazione dell'abbonamento {Id}", id);
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // PUT: api/Subscriptions/{id}/change-status
    [HttpPut("{id}/change-status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromQuery] int statusId)
    {
        try
        {
            var subscription = await _repository.GetSubscriptionByIdAsync(id);

            if (subscription == null)
            {
                await _auditService.LogErrorAsync("Subscriptions - ChangeStatus", $"Subscription {id} not found");
                return NotFound(new { success = false, error = "NOT_FOUND" });
            }

            var status = await _repository.GetStatusByIdAsync(statusId);
            if (status == null)
            {
                await _auditService.LogErrorAsync("Subscriptions - ChangeStatus", $"Status {statusId} not found");
                return BadRequest(new { success = false, error = "INVALID_STATUS_ID" });
            }

            subscription.StatusId = statusId;
            await _repository.UpdateSubscriptionAsync(subscription);

            return Ok(new { success = true, message = $"Stato cambiato in '{status.Name}'" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel cambio stato dell'abbonamento {Id}", id);
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // PUT: api/Subscriptions/{id}/renew
    [HttpPut("{id}/renew")]
    public async Task<IActionResult> RenewSubscription(int id, [FromQuery] int months = 12)
    {
        try
        {
            var subscription = await _repository.GetSubscriptionByIdAsync(id);

            if (subscription == null)
            {
                await _auditService.LogErrorAsync("Subscriptions - Renew", $"Subscription {id} not found");
                return NotFound(new { success = false, error = "NOT_FOUND" });
            }

            // Calcola nuova data di scadenza
            var currentEnd = subscription.EndDate ?? DateOnly.FromDateTime(DateTime.Today);
            var newEnd = currentEnd < DateOnly.FromDateTime(DateTime.Today)
                ? DateOnly.FromDateTime(DateTime.Today.AddMonths(months))
                : currentEnd.AddMonths(months);

            subscription.EndDate = newEnd;
            subscription.StatusId = 1; // active

            await _repository.UpdateSubscriptionAsync(subscription);

            return Ok(new {
                success = true,
                message = $"Abbonamento rinnovato fino al {newEnd:dd/MM/yyyy}",
                newEndDate = newEnd
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel rinnovo dell'abbonamento {Id}", id);
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // GET: api/Subscriptions/planes
    [HttpGet("planes")]
    public async Task<ActionResult> GetPlanes()
    {
        try
        {
            var planes = await _repository.GetPlanesAsync();
            if (planes == null)
            {
                await _auditService.LogActionAsync("Subscriptions", "GetPlanes", "No planes found");
                return Ok(new { success = true, data = Array.Empty<PlaneDto>() });
            }
            var result = planes.Select(p => new PlaneDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive ?? false
            });

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero dei piani");
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // GET: api/Subscriptions/statuses
    [HttpGet("statuses")]
    public async Task<ActionResult> GetStatuses()
    {
        try
        {
            var statuses = await _repository.GetStatusesAsync();
            if (statuses == null)
            {
                await _auditService.LogActionAsync("Subscriptions", "GetStatuses", "No statuses found");
                return Ok(new { success = true, data = Array.Empty<SubscriptionStatusDto>() });
            }
            var result = statuses.Select(s => new SubscriptionStatusDto
            {
                Id = s.Id,
                Name = s.Name,
                IsActive = s.IsActive ?? false
            });

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero degli stati");
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    // GET: api/Subscriptions/expiring
    [HttpGet("expiring")]
    public async Task<ActionResult> GetExpiringSubscriptions([FromQuery] int days = 30)
    {
        try
        {
            var subscriptions = await _repository.GetExpiringSubscriptionsAsync(days);
                        if (subscriptions == null)
            {
                await _auditService.LogActionAsync("Subscriptions", "GetExpiring", $"No expiring subscriptions found within {days} days");
                return Ok(new { success = true, data = Array.Empty<SubscriptionListDto>() });
            }
            var result = subscriptions.Select(s => new SubscriptionListDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                TenantName = s.Tenant.Name,
                TenantAcronym = s.Tenant.Acronym,
                PlaneId = s.PlaneId,
                PlaneName = s.Plane?.Name,
                StatusId = s.StatusId,
                StatusName = "active",
                StartDate = s.StartDate,
                EndDate = s.EndDate
            });

            return Ok(new { success = true, data = result, count = result.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero degli abbonamenti in scadenza");
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }
}
