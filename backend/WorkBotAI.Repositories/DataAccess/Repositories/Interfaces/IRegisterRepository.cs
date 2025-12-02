using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IRegisterRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<RegisterResponseDto> RegisterTenantAsync(RegisterTenantDto dto);
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }
}
