using Microsoft.AspNetCore.Mvc;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Persistence;
using WorkBotAI.API.Services;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkBotAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryPersistence _persistence;
        private readonly IAuditService _auditService;

        public CategoriesController(ICategoryPersistence persistence, IAuditService auditService)
        {
            _persistence = persistence;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult> GetCategories()
        {
            var data = await _persistence.GetAllAsync();
            return Ok(new { success = true, data });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetCategory(int id)
        {
            var data = await _persistence.GetByIdAsync(id);
            if (data == null) return NotFound(new { success = false, error = "Category not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost]
        public async Task<ActionResult> CreateCategory(CreateCategoryDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var success = await _persistence.CreateAsync(dto);
            if (success)
            {
                await _auditService.LogActionAsync("Categories", "Create", $"Created category {dto.Name}", null, null, userId);
                return Ok(new { success = true, message = "Category created" });
            }
            return StatusCode(500, new { success = false, error = "Error creating category" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var success = await _persistence.UpdateAsync(id, dto);
            if (success)
            {
                await _auditService.LogActionAsync("Categories", "Update", $"Updated category {dto.Name}", null, null, userId);
                return Ok(new { success = true, message = "Category updated" });
            }
            return NotFound(new { success = false, error = "Category not found or update error" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var success = await _persistence.DeleteAsync(id);
            if (success)
            {
                await _auditService.LogActionAsync("Categories", "Delete", $"Deleted category ID {id}", null, null, userId);
                return Ok(new { success = true, message = "Category deleted" });
            }
            return NotFound(new { success = false, error = "Category not found or delete error" });
        }
    }
}
