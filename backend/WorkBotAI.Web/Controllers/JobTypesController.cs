using Microsoft.AspNetCore.Mvc;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkBotAI.Web.Controllers
{
    public class JobTypesController : Controller
    {
        private readonly IJobTypeRepository _repository;

        public JobTypesController(IJobTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var jobTypes = await _repository.GetAllAsync();
            return View(jobTypes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var jobType = await _repository.GetByIdAsync(id);
            if (jobType == null) return NotFound();
            return View(jobType);
        }

        [HttpPost]
        public async Task<IActionResult> Create(JobType jobType)
        {
            if (ModelState.IsValid)
            {
                await _repository.CreateAsync(jobType);
                return RedirectToAction(nameof(Index));
            }
            return View(jobType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, JobType jobType)
        {
            if (id != jobType.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(jobType);
                return RedirectToAction(nameof(Index));
            }
            return View(jobType);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
