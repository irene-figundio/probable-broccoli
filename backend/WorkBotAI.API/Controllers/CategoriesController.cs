using Microsoft.AspNetCore.Mvc;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.Persistence.Repositories;
using WorkBotAI.API.Services;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkBotAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repository;
        private readonly IAuditService _auditService;

        public CategoriesController([FromKeyedServices("http")] ICategoryRepository repository, IAuditService auditService)
        {
            _repository = repository;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult> GetCategories()
        {
            var data = await _repository.GetAllAsync();
            return Ok(new { success = true, data });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetCategory(int id)
        {
            var data = await _repository.GetByIdAsync(id);
            if (data == null) return NotFound(new { success = false, error = "Category not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost]
        public async Task<ActionResult> CreateCategory(CreateCategoryDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var category = new Category { Name = dto.Name, IsActive = dto.IsActive };
            var created = await _repository.CreateAsync(category);

            await _auditService.LogActionAsync("Categories", "Create", $"Created category {created.Name}", null, null, userId);
            return Ok(new { success = true, data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var category = new Category { Id = id, Name = dto.Name, IsActive = dto.IsActive };
            await _repository.UpdateAsync(category);

            await _auditService.LogActionAsync("Categories", "Update", $"Updated category {dto.Name}", null, null, userId);
            return Ok(new { success = true, message = "Category updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            await _repository.DeleteAsync(id);

            await _auditService.LogActionAsync("Categories", "Delete", $"Deleted category ID {id}", null, null, userId);
            return Ok(new { success = true, message = "Category deleted" });
        }
    }
}
