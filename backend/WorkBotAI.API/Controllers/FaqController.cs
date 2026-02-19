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
public class FaqController : ControllerBase
{
    private readonly IFaqRepository _faqRepository;
    private readonly IAuditService _auditService;

    public FaqController(IFaqRepository faqRepository, IAuditService auditService)
    {
        _faqRepository = faqRepository;
        _auditService = auditService;
    }

    // GET: api/Faq
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaqListDto>>> GetFaqs([FromQuery] int? categoryId = null, [FromQuery] bool? isActive = null)
    {
        try
        {
            var faqs = await _faqRepository.GetFaqsAsync(categoryId, isActive);
            if (faqs == null) {
                await _auditService.LogErrorAsync("Faq - GetFaqs", "No FAQs found", null, null, null);
                return NotFound(new { success = false, error = "Nessuna FAQ trovata" });
            } else {
                await _auditService.LogActionAsync("Faq", "Get", $"Retrieved {faqs.Count()} FAQs", null, null);
            }
            var faqDtos = faqs.Select(f => new FaqListDto
            {
                Id = f.Id,
                CategoryId = f.CategoryId,
                CategoryName = f.Category != null ? f.Category.Name : null,
                Question = f.Question,
                IsActive = f.IsActive ?? true
            });
            return Ok(new { success = true, data = faqDtos });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Faq", "Error retrieving FAQs", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Faq/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<FaqDetailDto>> GetFaq(int id)
    {
        try
        {
            var faq = await _faqRepository.GetFaqByIdAsync(id);
            if (faq == null)
            {
                await _auditService.LogErrorAsync("Faq - GetFaq", $"FAQ not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "FAQ non trovata" });
            } else {
                await _auditService.LogActionAsync("Faq", "Get", $"Retrieved FAQ {id}", null, null);
            }

            var result = new FaqDetailDto
            {
                Id = faq.Id,
                CategoryId = faq.CategoryId,
                CategoryName = faq.Category?.Name,
                Question = faq.Question,
                IsActive = faq.IsActive ?? true,
                TenantAnswers = faq.TenantFaqs.Where(tf => tf.IsDeleted != true).Select(tf => new TenantFaqListDto
                {
                    Id = tf.Id,
                    TenantId = tf.TenantId,
                    TenantName = tf.Tenant?.Name,
                    FaqId = tf.FaqId,
                    Question = faq.Question,
                    Value = tf.Value,
                    IsActive = tf.IsActive ?? true
                }).ToList()
            };
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Faq", $"Error retrieving FAQ {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/Faq
    [HttpPost]
    public async Task<ActionResult> CreateFaq(CreateFaqDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Question))
            {
                await _auditService.LogErrorAsync("Faq - CreateFaq", "Question is required", null, null, null);
                return BadRequest(new { success = false, error = "La domanda è obbligatoria" });
            } 
            await _auditService.LogActionAsync("Faq", "Create", "Creating new FAQ", null, null);

            var faq = new Faq
            {
                CategoryId = dto.CategoryId,
                Question = dto.Question,
                IsActive = true
            };

            var createdFaq = await _faqRepository.CreateFaqAsync(faq);
            await _auditService.LogActionAsync("Faq", "Create", $"Created global FAQ {createdFaq.Id}: {createdFaq.Question}");

            return CreatedAtAction(nameof(GetFaq), new { id = createdFaq.Id }, new
            {
                success = true,
                data = new FaqListDto
                {
                    Id = createdFaq.Id,
                    CategoryId = createdFaq.CategoryId,
                    Question = createdFaq.Question,
                    IsActive = createdFaq.IsActive ?? true
                }
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Faq", "Error creating FAQ", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Faq/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFaq(int id, UpdateFaqDto dto)
    {
        try
        {
            var faq = await _faqRepository.GetFaqByIdAsync(id);
            if (faq == null)
            {
                await _auditService.LogErrorAsync("Faq - UpdateFaq", $"FAQ not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "FAQ non trovata" });
            }

            if (string.IsNullOrWhiteSpace(dto.Question))
            {
                await _auditService.LogErrorAsync("Faq - UpdateFaq", "Question is required", null, null, null);
                return BadRequest(new { success = false, error = "La domanda è obbligatoria" });
            }

            faq.CategoryId = dto.CategoryId;
            faq.Question = dto.Question;
            faq.IsActive = dto.IsActive;

            await _faqRepository.UpdateFaqAsync(faq);
            await _auditService.LogActionAsync("Faq", "Update", $"Updated global FAQ {id}");

            return Ok(new { success = true, message = "FAQ aggiornata con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Faq", $"Error updating FAQ {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/Faq/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFaq(int id)
    {
        try
        {
            var faq = await _faqRepository.GetFaqByIdAsync(id);
            if (faq == null)
            {
                await _auditService.LogErrorAsync("Faq - DeleteFaq", $"FAQ not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "FAQ non trovata" });
            } else {
                await _auditService.LogActionAsync("Faq", "Delete Attempt", $"Attempting to delete FAQ {id}", null, null);
            }

            await _faqRepository.DeleteFaqAsync(id);
            await _auditService.LogActionAsync("Faq", "Delete", $"Deleted global FAQ {id}");
            return Ok(new { success = true, message = "FAQ eliminata con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Faq", $"Error deleting FAQ {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Faq/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        try
        {
            var categories = await _faqRepository.GetCategoriesAsync();
            var categoryDtos = categories.Select(c => new { id = c.Id, name = c.Name });
            await _auditService.LogActionAsync("Faq", "GetCategories", "Retrieved FAQ categories", null, null);
            return Ok(new { success = true, data = categoryDtos });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Faq", "Error retrieving FAQ categories", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
