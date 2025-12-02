using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class FaqRepository : IFaqRepository
    {
        private readonly WorkBotAIContext _context;

        public FaqRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Faq>> GetFaqsAsync(int? categoryId, bool? isActive)
        {
            var query = _context.Faqs
                .Include(f => f.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(f => f.CategoryId == categoryId.Value);

            if (isActive.HasValue)
                query = query.Where(f => f.IsActive == isActive.Value);

            return await query
                .OrderBy(f => f.Category != null ? f.Category.Name : "")
                .ThenBy(f => f.Question)
                .ToListAsync();
        }

        public async Task<Faq> GetFaqByIdAsync(int id)
        {
            return await _context.Faqs
                .Include(f => f.Category)
                .Include(f => f.TenantFaqs)
                .ThenInclude(tf => tf.Tenant)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Faq> CreateFaqAsync(Faq faq)
        {
            _context.Faqs.Add(faq);
            await _context.SaveChangesAsync();
            return faq;
        }

        public async Task UpdateFaqAsync(Faq faq)
        {
            _context.Entry(faq).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFaqAsync(int id)
        {
            var faq = await _context.Faqs.FindAsync(id);
            if (faq != null)
            {
                var tenantFaqs = await _context.TenantFaqs.Where(tf => tf.FaqId == id).ToListAsync();
                _context.TenantFaqs.RemoveRange(tenantFaqs);
                _context.Faqs.Remove(faq);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
