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
    public class JobTypesController : ControllerBase
    {
        private readonly IJobTypeRepository _repository;
        private readonly IAuditService _auditService;

        public JobTypesController([FromKeyedServices("http")] IJobTypeRepository repository, IAuditService auditService)
        {
            _repository = repository;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult> GetJobTypes()
        {
            var data = await _repository.GetAllAsync();
            return Ok(new { success = true, data });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetJobType(int id)
        {
            var data = await _repository.GetByIdAsync(id);
            if (data == null) return NotFound(new { success = false, error = "JobType not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost]
        public async Task<ActionResult> CreateJobType(CreateJobTypeDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var jobType = new JobType { Name = dto.Name, Gender = dto.Gender, IsActive = dto.IsActive, CategoryId = dto.CategoryId };
            var created = await _repository.CreateAsync(jobType);

            await _auditService.LogActionAsync("JobTypes", "Create", $"Created JobType {created.Name}", null, null, userId);
            return Ok(new { success = true, data = created });
        }

        [HttpPost("bulk")]
        public async Task<ActionResult> BulkCreateJobType(BulkCreateJobTypeDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                userId = dto.UserId;
            }

            var jobTypesToCreate = new List<JobType>();
            if (dto.M) jobTypesToCreate.Add(new JobType { Name = dto.Descrizione, Gender = "M", CategoryId = dto.CategoryId, IsActive = true });
            if (dto.F) jobTypesToCreate.Add(new JobType { Name = dto.Descrizione, Gender = "F", CategoryId = dto.CategoryId, IsActive = true });
            if (dto.U) jobTypesToCreate.Add(new JobType { Name = dto.Descrizione, Gender = "U", CategoryId = dto.CategoryId, IsActive = true });

            if (!jobTypesToCreate.Any())
            {
                return BadRequest(new { success = false, error = "At least one gender flag (M, F, U) must be true" });
            }

            await _repository.CreateRangeAsync(jobTypesToCreate);
            await _auditService.LogActionAsync("JobTypes", "BulkCreate", $"Bulk created JobTypes for {dto.Descrizione}", null, null, userId);
            return Ok(new { success = true, message = "Bulk creation successful" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobType(int id, CreateJobTypeDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var jobType = new JobType { Id = id, Name = dto.Name, Gender = dto.Gender, IsActive = dto.IsActive, CategoryId = dto.CategoryId };
            await _repository.UpdateAsync(jobType);

            await _auditService.LogActionAsync("JobTypes", "Update", $"Updated JobType {dto.Name}", null, null, userId);
            return Ok(new { success = true, message = "JobType updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobType(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            await _repository.DeleteAsync(id);

            await _auditService.LogActionAsync("JobTypes", "Delete", $"Deleted JobType ID {id}", null, null, userId);
            return Ok(new { success = true, message = "JobType deleted" });
        }
    }
}
