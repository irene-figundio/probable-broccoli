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
public class TenantFaqController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public TenantFaqController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/TenantFaq
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantFaqListDto>>> GetTenantFaqs(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] int? faqId = null,
        [FromQuery] bool? isActive = null)
    {
        var query = _context.TenantFaqs
            .Where(tf => tf.IsDeleted != true)
            .Include(tf => tf.Tenant)
            .Include(tf => tf.Faq)
                .ThenInclude(f => f.Category)
            .AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(tf => tf.TenantId == tenantId.Value);
        }

        if (faqId.HasValue)
        {
            query = query.Where(tf => tf.FaqId == faqId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(tf => tf.IsActive == isActive.Value);
        }

        var tenantFaqs = await query
            .OrderBy(tf => tf.Faq.Category != null ? tf.Faq.Category.Name : "")
            .ThenBy(tf => tf.Faq.Question)
            .Select(tf => new TenantFaqListDto
            {
                Id = tf.Id,
                TenantId = tf.TenantId,
                TenantName = tf.Tenant.Name,
                FaqId = tf.FaqId,
                Question = tf.Faq.Question,
                CategoryName = tf.Faq.Category != null ? tf.Faq.Category.Name : null,
                Value = tf.Value,
                IsActive = tf.IsActive ?? true
            })
            .ToListAsync();

        return Ok(new { success = true, data = tenantFaqs });
    }

    // GET: api/TenantFaq/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TenantFaqDetailDto>> GetTenantFaq(int id)
    {
        var tenantFaq = await _context.TenantFaqs
            .Where(tf => tf.Id == id && tf.IsDeleted != true)
            .Include(tf => tf.Tenant)
            .Include(tf => tf.Faq)
                .ThenInclude(f => f.Category)
            .FirstOrDefaultAsync();

        if (tenantFaq == null)
        {
            return NotFound(new { success = false, error = "Risposta FAQ non trovata" });
        }

        var result = new TenantFaqDetailDto
        {
            Id = tenantFaq.Id,
            TenantId = tenantFaq.TenantId,
            TenantName = tenantFaq.Tenant?.Name,
            FaqId = tenantFaq.FaqId,
            Question = tenantFaq.Faq?.Question,
            CategoryName = tenantFaq.Faq?.Category?.Name,
            Value = tenantFaq.Value,
            IsActive = tenantFaq.IsActive ?? true,
            CreationTime = tenantFaq.CreationTime,
            LastModificationTime = tenantFaq.LastModificationTime
        };

        return Ok(new { success = true, data = result });
    }

    // POST: api/TenantFaq
    [HttpPost]
    public async Task<ActionResult> CreateTenantFaq(CreateTenantFaqDto dto)
    {
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
        {
            return BadRequest(new { success = false, error = "Tenant non trovato" });
        }

        var faq = await _context.Faqs.FindAsync(dto.FaqId);
        if (faq == null)
        {
            return BadRequest(new { success = false, error = "FAQ non trovata" });
        }

        var existing = await _context.TenantFaqs
            .FirstOrDefaultAsync(tf => tf.TenantId == dto.TenantId && tf.FaqId == dto.FaqId && tf.IsDeleted != true);

        if (existing != null)
        {
            return BadRequest(new { success = false, error = "Risposta già esistente per questa FAQ" });
        }

        var tenantFaq = new TenantFaq
        {
            TenantId = dto.TenantId,
            FaqId = dto.FaqId,
            Value = dto.Value,
            IsActive = true,
            CreationTime = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.TenantFaqs.Add(tenantFaq);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTenantFaq), new { id = tenantFaq.Id }, new
        {
            success = true,
            data = new TenantFaqListDto
            {
                Id = tenantFaq.Id,
                TenantId = tenantFaq.TenantId,
                TenantName = tenant.Name,
                FaqId = tenantFaq.FaqId,
                Question = faq.Question,
                Value = tenantFaq.Value,
                IsActive = tenantFaq.IsActive ?? true
            }
        });
    }

    // PUT: api/TenantFaq/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenantFaq(int id, UpdateTenantFaqDto dto)
    {
        var tenantFaq = await _context.TenantFaqs.FindAsync(id);

        if (tenantFaq == null || tenantFaq.IsDeleted == true)
        {
            return NotFound(new { success = false, error = "Risposta FAQ non trovata" });
        }

        tenantFaq.Value = dto.Value;
        tenantFaq.IsActive = dto.IsActive;
        tenantFaq.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Risposta FAQ aggiornata con successo" });
    }

    // DELETE: api/TenantFaq/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenantFaq(int id)
    {
        var tenantFaq = await _context.TenantFaqs.FindAsync(id);

        if (tenantFaq == null)
        {
            return NotFound(new { success = false, error = "Risposta FAQ non trovata" });
        }

        tenantFaq.IsDeleted = true;
        tenantFaq.DeletionTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Risposta FAQ eliminata con successo" });
    }

    // PUT: api/TenantFaq/tenant/{tenantId}/batch
    [HttpPut("tenant/{tenantId}/batch")]
    public async Task<IActionResult> UpsertTenantFaqs(Guid tenantId, [FromBody] List<UpsertTenantFaqDto> dtos)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            return BadRequest(new { success = false, error = "Tenant non trovato" });
        }

        foreach (var dto in dtos)
        {
            var faq = await _context.Faqs.FindAsync(dto.FaqId);
            if (faq == null) continue;

            var existing = await _context.TenantFaqs
                .FirstOrDefaultAsync(tf => tf.TenantId == tenantId && tf.FaqId == dto.FaqId && tf.IsDeleted != true);

            if (existing != null)
            {
                existing.Value = dto.Value;
                existing.LastModificationTime = DateTime.UtcNow;
            }
            else
            {
                var tenantFaq = new TenantFaq
                {
                    TenantId = tenantId,
                    FaqId = dto.FaqId,
                    Value = dto.Value,
                    IsActive = true,
                    CreationTime = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.TenantFaqs.Add(tenantFaq);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "FAQ aggiornate con successo" });
    }

    // GET: api/TenantFaq/tenant/{tenantId}/chatbot
    [HttpGet("tenant/{tenantId}/chatbot")]
    public async Task<ActionResult> GetChatbotFaqs(Guid tenantId)
    {
        var faqs = await _context.TenantFaqs
            .Where(tf => tf.TenantId == tenantId && tf.IsDeleted != true && tf.IsActive == true)
            .Include(tf => tf.Faq)
                .ThenInclude(f => f.Category)
            .Where(tf => tf.Faq.IsActive == true)
            .OrderBy(tf => tf.Faq.Category != null ? tf.Faq.Category.Name : "")
            .ThenBy(tf => tf.Faq.Question)
            .Select(tf => new ChatbotFaqDto
            {
                Question = tf.Faq.Question,
                Answer = tf.Value,
                Category = tf.Faq.Category != null ? tf.Faq.Category.Name : null
            })
            .ToListAsync();

        return Ok(new { success = true, data = faqs });
    }

    // GET: api/TenantFaq/tenant/{tenantId}/with-all-faqs
    [HttpGet("tenant/{tenantId}/with-all-faqs")]
    public async Task<ActionResult> GetTenantFaqsWithAllFaqs(Guid tenantId)
    {
        var allFaqs = await _context.Faqs
            .Where(f => f.IsActive == true)
            .Include(f => f.Category)
            .OrderBy(f => f.Category != null ? f.Category.Name : "")
            .ThenBy(f => f.Question)
            .ToListAsync();

        var tenantAnswers = await _context.TenantFaqs
            .Where(tf => tf.TenantId == tenantId && tf.IsDeleted != true)
            .ToListAsync();

        var result = allFaqs.Select(f => new
        {
            faqId = f.Id,
            question = f.Question,
            categoryId = f.CategoryId,
            categoryName = f.Category?.Name,
            tenantFaqId = tenantAnswers.FirstOrDefault(ta => ta.FaqId == f.Id)?.Id,
            value = tenantAnswers.FirstOrDefault(ta => ta.FaqId == f.Id)?.Value,
            isAnswered = tenantAnswers.Any(ta => ta.FaqId == f.Id),
            isActive = tenantAnswers.FirstOrDefault(ta => ta.FaqId == f.Id)?.IsActive ?? false
        }).ToList();

        return Ok(new { success = true, data = result });
    }

    // ============================================
    // SIMPLE FAQ MANAGEMENT (standalone per tenant)
    // ============================================

    // GET: api/TenantFaq/tenant/{tenantId}
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult> GetSimpleTenantFaqs(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            return NotFound(new { success = false, error = "Tenant non trovato" });

        var faqs = await _context.TenantFaqs
            .Where(tf => tf.TenantId == tenantId && tf.IsDeleted != true)
            .Include(tf => tf.Faq)
                .ThenInclude(f => f.Category)
            .OrderBy(tf => tf.Faq.Category != null ? tf.Faq.Category.Name : "")
            .ThenBy(tf => tf.Faq.Question)
            .Select(tf => new 
            {
                id = tf.Id,
                tenantId = tf.TenantId,
                question = tf.Faq.Question,
                answer = tf.Value,
                category = tf.Faq.Category != null ? tf.Faq.Category.Name : "Generale",
                isActive = tf.IsActive ?? true,
                sortOrder = tf.Faq.Id,
                createdAt = tf.CreationTime
            })
            .ToListAsync();

        return Ok(new { success = true, data = faqs });
    }

    // POST: api/TenantFaq/simple
    [HttpPost("simple")]
    public async Task<ActionResult> CreateSimpleFaq([FromBody] SimpleFaqDto dto)
    {
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
            return BadRequest(new { success = false, error = "Tenant non trovato" });

        // Trova o crea la categoria
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == dto.Category);
        if (category == null)
        {
            category = new Category { Name = dto.Category ?? "Generale", IsActive = true };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        // Crea la FAQ globale (solo Question, la risposta va in TenantFaq.Value)
        var faq = new Faq
        {
            Question = dto.Question,
            CategoryId = category.Id,
            IsActive = true
        };
        _context.Faqs.Add(faq);
        await _context.SaveChangesAsync();

        // Crea il collegamento TenantFaq (la risposta è in Value)
        var tenantFaq = new TenantFaq
        {
            TenantId = dto.TenantId,
            FaqId = faq.Id,
            Value = dto.Answer,
            IsActive = dto.IsActive,
            CreationTime = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.TenantFaqs.Add(tenantFaq);
        await _context.SaveChangesAsync();

        return Ok(new { 
            success = true, 
            data = new { 
                id = tenantFaq.Id,
                tenantId = tenantFaq.TenantId,
                question = faq.Question,
                answer = tenantFaq.Value,
                category = category.Name,
                isActive = tenantFaq.IsActive
            }
        });
    }

    // PUT: api/TenantFaq/simple/{id}
    [HttpPut("simple/{id}")]
    public async Task<ActionResult> UpdateSimpleFaq(int id, [FromBody] SimpleFaqDto dto)
    {
        var tenantFaq = await _context.TenantFaqs
            .Include(tf => tf.Faq)
            .FirstOrDefaultAsync(tf => tf.Id == id && tf.IsDeleted != true);

        if (tenantFaq == null)
            return NotFound(new { success = false, error = "FAQ non trovata" });

        // Aggiorna la FAQ (solo Question)
        if (tenantFaq.Faq != null)
        {
            tenantFaq.Faq.Question = dto.Question;
            
            // Aggiorna categoria se cambiata
            if (!string.IsNullOrEmpty(dto.Category))
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == dto.Category);
                if (category == null)
                {
                    category = new Category { Name = dto.Category, IsActive = true };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
                tenantFaq.Faq.CategoryId = category.Id;
            }
        }

        // La risposta va in Value
        tenantFaq.Value = dto.Answer;
        tenantFaq.IsActive = dto.IsActive;
        tenantFaq.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "FAQ aggiornata" });
    }
}