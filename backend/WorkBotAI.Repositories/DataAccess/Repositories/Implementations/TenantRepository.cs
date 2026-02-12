using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class TenantRepository : ITenantRepository
    {
        private readonly WorkBotAIContext _context;

        public TenantRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tenant>> GetTenantsAsync()
        {
            return await _context.Tenants
                .Where(t => t.IsDeleted != true)
                .Include(t => t.Category)
                .ToListAsync();
        }

        public async Task<Tenant?> GetTenantByIdAsync(Guid id)
        {
            return await _context.Tenants
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted != true);
        }

        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        public async Task UpdateTenantAsync(Tenant tenant)
        {
            _context.Entry(tenant).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTenantAsync(Guid id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant != null)
            {
                tenant.IsDeleted = true;
                tenant.DeletionTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive == true)
                .ToListAsync();
        }
    }
}
