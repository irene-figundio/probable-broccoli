using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
    }
}
