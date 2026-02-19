using System.Collections.Generic;
using System.Threading.Tasks;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.API.Persistence
{
    public interface IJobTypePersistence
    {
        Task<IEnumerable<JobTypeDto>> GetAllAsync();
        Task<JobTypeDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateJobTypeDto dto);
        Task<bool> BulkCreateAsync(BulkCreateJobTypeDto dto);
        Task<bool> UpdateAsync(int id, CreateJobTypeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
