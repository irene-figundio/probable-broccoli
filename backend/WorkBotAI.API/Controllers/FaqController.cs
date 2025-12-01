using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public FaqController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Faq
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FaqListDto>>> GetFaqs(
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? isActive = null)
    {
        var query = _context.Faqs
            .Include(f => f.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.CategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(f => f.IsActive == isActive.Value);
        }

        var faqs = await query
            .OrderBy(f => f.Category != null ? f.Category.Name : "")
            .ThenBy(f => f.Question)
            .Select(f => new FaqListDto
            {
                Id = f.Id,
                CategoryId = f.CategoryId,
                CategoryName = f.Category != null ? f.Category.Name : null,
                Question = f.Question,
                IsActive = f.IsActive ?? true
            })
            .ToListAsync();

        return Ok(new { success = true, data = faqs });
    }

    // GET: api/Faq/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<FaqDetailDto>> GetFaq(int id)
    {
        var faq = await _context.Faqs
            .Include(f => f.Category)
            .Include(f => f.TenantFaqs)
                .ThenInclude(tf => tf.Tenant)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (faq == null)
        {
            return NotFound(new { success = false, error = "FAQ non trovata" });
        }

        var result = new FaqDetailDto
        {
            Id = faq.Id,
            CategoryId = faq.CategoryId,
            CategoryName = faq.Category?.Name,
            Question = faq.Question,
            IsActive = faq.IsActive ?? true,
            TenantAnswers = faq.TenantFaqs
                .Where(tf => tf.IsDeleted != true)
                .Select(tf => new TenantFaqListDto
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
        {
            return BadRequest(new { success = false, error = "La domanda è obbligatoria" });
        }

        // Verifica categoria se specificata
        if (dto.CategoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
            if (category == null)
            {
                return BadRequest(new { success = false, error = "Categoria non trovata" });
            }
        }

        var faq = new Faq
        {
            CategoryId = dto.CategoryId,
            Question = dto.Question,
            IsActive = true
        };

        _context.Faqs.Add(faq);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFaq), new { id = faq.Id }, new
        {
            success = true,
            data = new FaqListDto
            {
                Id = faq.Id,
                CategoryId = faq.CategoryId,
                Question = faq.Question,
                IsActive = faq.IsActive ?? true
            }
        });
    }

    // PUT: api/Faq/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFaq(int id, UpdateFaqDto dto)
    {
        var faq = await _context.Faqs.FindAsync(id);

        if (faq == null)
        {
            return NotFound(new { success = false, error = "FAQ non trovata" });
        }

        if (string.IsNullOrWhiteSpace(dto.Question))
        {
            return BadRequest(new { success = false, error = "La domanda è obbligatoria" });
        }

        // Verifica categoria se specificata
        if (dto.CategoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
            if (category == null)
            {
                return BadRequest(new { success = false, error = "Categoria non trovata" });
            }
        }

        faq.CategoryId = dto.CategoryId;
        faq.Question = dto.Question;
        faq.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "FAQ aggiornata con successo" });
    }

    // DELETE: api/Faq/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFaq(int id)
    {
        var faq = await _context.Faqs.FindAsync(id);

        if (faq == null)
        {
            return NotFound(new { success = false, error = "FAQ non trovata" });
        }

        // Rimuovi anche le risposte dei tenant
        var tenantFaqs = await _context.TenantFaqs.Where(tf => tf.FaqId == id).ToListAsync();
        _context.TenantFaqs.RemoveRange(tenantFaqs);

        _context.Faqs.Remove(faq);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "FAQ eliminata con successo" });
    }

    // GET: api/Faq/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive == true)
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name
            })
            .ToListAsync();

        return Ok(new { success = true, data = categories });
    }
}