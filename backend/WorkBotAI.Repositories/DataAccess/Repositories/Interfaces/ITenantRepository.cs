using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface ITenantRepository
    {
        Task<IEnumerable<Tenant>> GetTenantsAsync();
        Task<Tenant?> GetTenantByIdAsync(Guid id);
        Task<Tenant> CreateTenantAsync(Tenant tenant);
        Task UpdateTenantAsync(Tenant tenant);
        Task DeleteTenantAsync(Guid id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
    }
}
