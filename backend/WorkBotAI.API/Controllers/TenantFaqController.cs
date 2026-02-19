using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Services;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TenantFaqController : ControllerBase
{
    private readonly ITenantFaqRepository _repository;
    private readonly IAuditService _auditService;

    public TenantFaqController(ITenantFaqRepository repository, IAuditService auditService)
    {
        _repository = repository;
        _auditService = auditService;
    }

    [HttpGet("{tenantId}")]
    public async Task<ActionResult<IEnumerable<TenantFaqDto>>> GetTenantFaqs(Guid tenantId)
    {
        try
        {
            var faqs = await _repository.GetTenantFaqsAsync(tenantId);
            if (faqs == null || !faqs.Any())
                {
                await _auditService.LogActionAsync("TenantFaq", "Get", "No FAQs found for tenant", null, tenantId);
                return Ok(new { success = true, data = new List<TenantFaqDto>() });
            }
            var dtos = faqs.Select(tf => new TenantFaqDto
            {
                Id = tf.Id,
                TenantId = tf.TenantId,
                FaqId = tf.FaqId,
                Question = tf.Faq?.Question ?? "",
                CategoryName = tf.Faq?.Category?.Name ?? "",
                Value = tf.Value,
                IsActive = tf.IsActive ?? true
            });

            return Ok(new { success = true, data = dtos });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("TenantFaq", "Error retrieving tenant FAQs", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateTenantFaq(CreateTenantFaqDto dto)
    {
        try
        {
            var tenantFaq = new TenantFaq
            {
                TenantId = dto.TenantId,
                FaqId = dto.FaqId,
                Value = dto.Value,
                IsActive = true,
                CreationTime = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.CreateAsync(tenantFaq);
            await _auditService.LogActionAsync("TenantFaq", "Create", $"Created FAQ answer for FAQ {dto.FaqId}", null, dto.TenantId);

            return Ok(new { success = true, data = new { id = tenantFaq.Id } });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("TenantFaq", "Error creating tenant FAQ", ex, dto.TenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenantFaq(int id, UpdateTenantFaqDto dto)
    {
        try
        {
            var tenantFaq = await _repository.GetByIdAsync(id);
            if (tenantFaq == null)
            {
                await _auditService.LogErrorAsync("TenantFaq - Update", $"FAQ with id {id} not found for update", null, null);
                return NotFound(new { success = false, error = "FAQ non trovata" });
            }

            tenantFaq.Value = dto.Value;
            tenantFaq.IsActive = dto.IsActive;
            tenantFaq.LastModificationTime = DateTime.UtcNow;

            await _repository.UpdateAsync(tenantFaq);
            await _auditService.LogActionAsync("TenantFaq", "Update", $"Updated FAQ answer {id}", null, tenantFaq.TenantId);

            return Ok(new { success = true, message = "FAQ aggiornata con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("TenantFaq", $"Error updating tenant FAQ {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenantFaq(int id)
    {
        try
        {
            var tenantFaq = await _repository.GetByIdAsync(id);
            if (tenantFaq == null)
            {
                await _auditService.LogErrorAsync("TenantFaq - Delete", $"FAQ with id {id} not found for deletion", null, null);
                return NotFound(new { success = false, error = "FAQ non trovata" });
            }

            await _repository.DeleteAsync(id);
            await _auditService.LogActionAsync("TenantFaq", "Delete", $"Deleted FAQ answer {id}", null, tenantFaq.TenantId);

            return Ok(new { success = true, message = "FAQ eliminata con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("TenantFaq", $"Error deleting tenant FAQ {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    [HttpGet("global")]
    public async Task<ActionResult> GetGlobalFaqs()
    {
        try
        {
            var faqs = await _repository.GetGlobalFaqsAsync();
            if (faqs == null)
            {
                await _auditService.LogActionAsync("TenantFaq", "GetGlobal", "No global FAQs found");
                return Ok(new { success = true, data = new List<object>() });
            }
            return Ok(new { success = true, data = faqs.Select(f => new { f.Id, f.Question, Category = f.Category?.Name }) });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("TenantFaq", "Error retrieving global FAQs", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
