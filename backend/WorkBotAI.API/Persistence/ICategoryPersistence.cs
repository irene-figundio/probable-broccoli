using System.Collections.Generic;
using System.Threading.Tasks;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.API.Persistence
{
    public interface ICategoryPersistence
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateCategoryDto dto);
        Task<bool> UpdateAsync(int id, CategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
