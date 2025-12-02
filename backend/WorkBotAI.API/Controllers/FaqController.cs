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
public class FaqController : ControllerBase
{
    private readonly IFaqRepository _faqRepository;

    public FaqController(IFaqRepository faqRepository)
    {
        _faqRepository = faqRepository;
    }

    // GET: api/Faq
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaqListDto>>> GetFaqs([FromQuery] int? categoryId = null, [FromQuery] bool? isActive = null)
    {
        var faqs = await _faqRepository.GetFaqsAsync(categoryId, isActive);
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

    // GET: api/Faq/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<FaqDetailDto>> GetFaq(int id)
    {
        var faq = await _faqRepository.GetFaqByIdAsync(id);
        if (faq == null)
            return NotFound(new { success = false, error = "FAQ non trovata" });

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

    // POST: api/Faq
    [HttpPost]
    public async Task<ActionResult> CreateFaq(CreateFaqDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Question))
            return BadRequest(new { success = false, error = "La domanda è obbligatoria" });

        var faq = new Faq
        {
            CategoryId = dto.CategoryId,
            Question = dto.Question,
            IsActive = true
        };

        var createdFaq = await _faqRepository.CreateFaqAsync(faq);
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

    // PUT: api/Faq/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFaq(int id, UpdateFaqDto dto)
    {
        var faq = await _faqRepository.GetFaqByIdAsync(id);
        if (faq == null)
            return NotFound(new { success = false, error = "FAQ non trovata" });

        if (string.IsNullOrWhiteSpace(dto.Question))
            return BadRequest(new { success = false, error = "La domanda è obbligatoria" });

        faq.CategoryId = dto.CategoryId;
        faq.Question = dto.Question;
        faq.IsActive = dto.IsActive;

        await _faqRepository.UpdateFaqAsync(faq);
        return Ok(new { success = true, message = "FAQ aggiornata con successo" });
    }

    // DELETE: api/Faq/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFaq(int id)
    {
        await _faqRepository.DeleteFaqAsync(id);
        return Ok(new { success = true, message = "FAQ eliminata con successo" });
    }

    // GET: api/Faq/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _faqRepository.GetCategoriesAsync();
        var categoryDtos = categories.Select(c => new { id = c.Id, name = c.Name });
        return Ok(new { success = true, data = categoryDtos });
    }
}