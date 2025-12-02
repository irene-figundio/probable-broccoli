using System.Threading.Tasks;
using WorkBotAI.Repositories.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<AdminStatsDto> GetAdminStatsAsync();
    }
}
