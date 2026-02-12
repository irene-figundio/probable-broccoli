using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class TenantFaqRepository : ITenantFaqRepository
    {
        private readonly WorkBotAIContext _context;

        public TenantFaqRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TenantFaq>> GetTenantFaqsAsync(Guid tenantId)
        {
            return await _context.TenantFaqs
                .Where(tf => tf.TenantId == tenantId && tf.IsDeleted != true)
                .Include(tf => tf.Faq)
                    .ThenInclude(f => f.Category)
                .ToListAsync();
        }

        public async Task<TenantFaq?> GetByIdAsync(int id)
        {
            return await _context.TenantFaqs
                .Where(tf => tf.Id == id && tf.IsDeleted != true)
                .Include(tf => tf.Faq)
                .FirstOrDefaultAsync();
        }

        public async Task<TenantFaq> CreateAsync(TenantFaq tenantFaq)
        {
            _context.TenantFaqs.Add(tenantFaq);
            await _context.SaveChangesAsync();
            return tenantFaq;
        }

        public async Task UpdateAsync(TenantFaq tenantFaq)
        {
            _context.Entry(tenantFaq).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tf = await _context.TenantFaqs.FindAsync(id);
            if (tf != null)
            {
                tf.IsDeleted = true;
                tf.DeletionTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Faq>> GetGlobalFaqsAsync()
        {
            return await _context.Faqs
                .Where(f => f.IsActive == true)
                .Include(f => f.Category)
                .ToListAsync();
        }
    }
}
