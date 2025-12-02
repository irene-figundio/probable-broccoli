using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IFaqRepository
    {
        Task<IEnumerable<Faq>> GetFaqsAsync(int? categoryId, bool? isActive);
        Task<Faq> GetFaqByIdAsync(int id);
        Task<Faq> CreateFaqAsync(Faq faq);
        Task UpdateFaqAsync(Faq faq);
        Task DeleteFaqAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }
}
