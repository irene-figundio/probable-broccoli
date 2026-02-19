using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class JobTypeRepository : IJobTypeRepository
    {
        private readonly WorkBotAIContext _context;

        public JobTypeRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobType>> GetAllAsync()
        {
            return await _context.JobTypes.Include(j => j.Category).ToListAsync();
        }

        public async Task<JobType?> GetByIdAsync(int id)
        {
            return await _context.JobTypes.Include(j => j.Category).FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<JobType> CreateAsync(JobType jobType)
        {
            _context.JobTypes.Add(jobType);
            await _context.SaveChangesAsync();
            return jobType;
        }

        public async Task UpdateAsync(JobType jobType)
        {
            _context.Entry(jobType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var jobType = await _context.JobTypes.FindAsync(id);
            if (jobType != null)
            {
                _context.JobTypes.Remove(jobType);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreateRangeAsync(IEnumerable<JobType> jobTypes)
        {
            await _context.JobTypes.AddRangeAsync(jobTypes);
            await _context.SaveChangesAsync();
        }
    }
}
