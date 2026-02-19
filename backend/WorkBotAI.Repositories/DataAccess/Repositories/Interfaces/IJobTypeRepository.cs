using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IJobTypeRepository
    {
        Task<IEnumerable<JobType>> GetAllAsync();
        Task<JobType?> GetByIdAsync(int id);
        Task<JobType> CreateAsync(JobType jobType);
        Task UpdateAsync(JobType jobType);
        Task DeleteAsync(int id);
        Task CreateRangeAsync(IEnumerable<JobType> jobTypes);
    }
}
