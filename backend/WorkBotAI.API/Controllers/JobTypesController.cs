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
    public class JobTypesController : ControllerBase
    {
        private readonly IJobTypePersistence _persistence;
        private readonly IAuditService _auditService;

        public JobTypesController(IJobTypePersistence persistence, IAuditService auditService)
        {
            _persistence = persistence;
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult> GetJobTypes()
        {
            var data = await _persistence.GetAllAsync();
            return Ok(new { success = true, data });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetJobType(int id)
        {
            var data = await _persistence.GetByIdAsync(id);
            if (data == null) return NotFound(new { success = false, error = "JobType not found" });
            return Ok(new { success = true, data });
        }

        [HttpPost]
        public async Task<ActionResult> CreateJobType(CreateJobTypeDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var success = await _persistence.CreateAsync(dto);
            if (success)
            {
                await _auditService.LogActionAsync("JobTypes", "Create", $"Created JobType {dto.Name}", null, null, userId);
                return Ok(new { success = true, message = "JobType created" });
            }
            return StatusCode(500, new { success = false, error = "Error creating JobType" });
        }

        [HttpPost("bulk")]
        public async Task<ActionResult> BulkCreateJobType(BulkCreateJobTypeDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                userId = dto.UserId;
            }

            var success = await _persistence.BulkCreateAsync(dto);
            if (success)
            {
                await _auditService.LogActionAsync("JobTypes", "BulkCreate", $"Bulk created JobTypes for {dto.Descrizione}", null, null, userId);
                return Ok(new { success = true, message = "Bulk creation successful" });
            }
            return StatusCode(500, new { success = false, error = "Error in bulk creation" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobType(int id, CreateJobTypeDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var success = await _persistence.UpdateAsync(id, dto);
            if (success)
            {
                await _auditService.LogActionAsync("JobTypes", "Update", $"Updated JobType {dto.Name}", null, null, userId);
                return Ok(new { success = true, message = "JobType updated" });
            }
            return NotFound(new { success = false, error = "JobType not found or update error" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobType(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var success = await _persistence.DeleteAsync(id);
            if (success)
            {
                await _auditService.LogActionAsync("JobTypes", "Delete", $"Deleted JobType ID {id}", null, null, userId);
                return Ok(new { success = true, message = "JobType deleted" });
            }
            return NotFound(new { success = false, error = "JobType not found or delete error" });
        }
    }
}
