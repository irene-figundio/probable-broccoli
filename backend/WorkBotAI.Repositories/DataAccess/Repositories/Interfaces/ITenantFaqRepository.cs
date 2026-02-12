using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface ITenantFaqRepository
    {
        Task<IEnumerable<TenantFaq>> GetTenantFaqsAsync(Guid tenantId);
        Task<TenantFaq?> GetByIdAsync(int id);
        Task<TenantFaq> CreateAsync(TenantFaq tenantFaq);
        Task UpdateAsync(TenantFaq tenantFaq);
        Task DeleteAsync(int id);
        Task<IEnumerable<Faq>> GetGlobalFaqsAsync();
    }
}
