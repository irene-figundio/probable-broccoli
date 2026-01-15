using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        Task<IEnumerable<ServiceListDto>> GetServicesAsync(Guid? tenantId, string search, int? categoryId);
        Task<Service> GetServiceByIdAsync(int id);
        Task<Service> CreateServiceAsync(Service service);
        Task UpdateServiceAsync(Service service);
        Task DeleteServiceAsync(int id);
        Task ToggleServiceStatusAsync(int id);
        Task<int> GetTotalServicesAsync(Guid tenantId);
        Task<int> GetActiveServicesAsync(Guid tenantId);
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }
}
