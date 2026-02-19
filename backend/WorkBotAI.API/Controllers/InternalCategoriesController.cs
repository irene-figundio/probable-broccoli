using Microsoft.AspNetCore.Mvc;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkBotAI.API.Controllers
{
    [ApiController]
    [Route("api/internal/categories")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InternalCategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repository;

        public InternalCategoriesController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _repository.GetAllAsync();
            return Ok(new { success = true, data = categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, IsActive = c.IsActive ?? false }) });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return NotFound(new { success = false, error = "Category not found" });
            return Ok(new { success = true, data = new CategoryDto { Id = category.Id, Name = category.Name, IsActive = category.IsActive ?? false } });
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                IsActive = dto.IsActive
            };

            var created = await _repository.CreateAsync(category);
            return CreatedAtAction(nameof(GetCategory), new { id = created.Id }, new { success = true, data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return NotFound(new { success = false, error = "Category not found" });

            category.Name = dto.Name;
            category.IsActive = dto.IsActive;

            await _repository.UpdateAsync(category);
            return Ok(new { success = true, message = "Category updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return NotFound(new { success = false, error = "Category not found" });

            await _repository.DeleteAsync(id);
            return Ok(new { success = true, message = "Category deleted" });
        }
    }
}
