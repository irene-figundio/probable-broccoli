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
    [Route("api/internal/jobtypes")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InternalJobTypesController : ControllerBase
    {
        private readonly IJobTypeRepository _repository;

        public InternalJobTypesController(IJobTypeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobTypeDto>>> GetJobTypes()
        {
            var jobTypes = await _repository.GetAllAsync();
            return Ok(new { success = true, data = jobTypes.Select(j => new JobTypeDto {
                Id = j.Id,
                Name = j.Name,
                IsActive = j.IsActive ?? false,
                CategoryId = j.CategoryId,
                Gender = j.Gender
            }) });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobTypeDto>> GetJobType(int id)
        {
            var jobType = await _repository.GetByIdAsync(id);
            if (jobType == null) return NotFound(new { success = false, error = "JobType not found" });
            return Ok(new { success = true, data = new JobTypeDto {
                Id = jobType.Id,
                Name = jobType.Name,
                IsActive = jobType.IsActive ?? false,
                CategoryId = jobType.CategoryId,
                Gender = jobType.Gender
            } });
        }

        [HttpPost]
        public async Task<ActionResult> CreateJobType(CreateJobTypeDto dto)
        {
            var jobType = new JobType
            {
                Name = dto.Name,
                IsActive = dto.IsActive,
                CategoryId = dto.CategoryId,
                Gender = dto.Gender
            };

            var created = await _repository.CreateAsync(jobType);
            return CreatedAtAction(nameof(GetJobType), new { id = created.Id }, new { success = true, data = created });
        }

        [HttpPost("bulk")]
        public async Task<ActionResult> BulkCreateJobType(BulkCreateJobTypeDto dto)
        {
            var jobTypesToCreate = new List<JobType>();

            if (dto.M) jobTypesToCreate.Add(new JobType { Name = dto.Descrizione, Gender = "M", CategoryId = dto.CategoryId, IsActive = true });
            if (dto.F) jobTypesToCreate.Add(new JobType { Name = dto.Descrizione, Gender = "F", CategoryId = dto.CategoryId, IsActive = true });
            if (dto.U) jobTypesToCreate.Add(new JobType { Name = dto.Descrizione, Gender = "U", CategoryId = dto.CategoryId, IsActive = true });

            if (!jobTypesToCreate.Any())
            {
                return BadRequest(new { success = false, error = "At least one gender flag (M, F, U) must be true" });
            }

            await _repository.CreateRangeAsync(jobTypesToCreate);
            return Ok(new { success = true, count = jobTypesToCreate.Count });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobType(int id, CreateJobTypeDto dto)
        {
            var jobType = await _repository.GetByIdAsync(id);
            if (jobType == null) return NotFound(new { success = false, error = "JobType not found" });

            jobType.Name = dto.Name;
            jobType.IsActive = dto.IsActive;
            jobType.CategoryId = dto.CategoryId;
            jobType.Gender = dto.Gender;

            await _repository.UpdateAsync(jobType);
            return Ok(new { success = true, message = "JobType updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobType(int id)
        {
            var jobType = await _repository.GetByIdAsync(id);
            if (jobType == null) return NotFound(new { success = false, error = "JobType not found" });

            await _repository.DeleteAsync(id);
            return Ok(new { success = true, message = "JobType deleted" });
        }
    }
}
