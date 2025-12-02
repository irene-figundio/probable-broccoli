using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly WorkBotAIContext _context;

        public ServiceRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceListDto>> GetServicesAsync(Guid? tenantId, string search, int? categoryId)
        {
            var query = _context.Services
                .Where(s => s.IsDeleted != true)
                .Include(s => s.Tenant)
                .Include(s => s.Category)
                .Select(s => new ServiceListDto
                {
                    Id = s.Id,
                    TenantId = s.TenantId,
                    TenantName = s.Tenant != null ? s.Tenant.Name : null,
                    Name = s.Name,
                    Description = s.Description,
                    CategoryName = s.Category != null ? s.Category.Name : null,
                    DurationMin = s.DurationMin,
                    BasePrice = s.BasePrice,
                    IsActive = s.IsActive ?? true,
                    CreationTime = s.CreationTime,
                    AppointmentsCount = s.AppointmentServices.Count()
                });

            if (tenantId.HasValue) query = query.Where(s => s.TenantId == tenantId.Value);
            if (categoryId.HasValue) query = query.Where(s => s.CategoryId == categoryId.Value);
            if (!string.IsNullOrEmpty(search)) query = query.Where(s => (s.Name != null && s.Name.Contains(search)) || (s.Description != null && s.Description.Contains(search)));

            return await query.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<Service> GetServiceByIdAsync(int id)
        {
            return await _context.Services
                .Where(s => s.Id == id && s.IsDeleted != true)
                .Include(s => s.Tenant)
                .Include(s => s.Category)
                .FirstOrDefaultAsync();
        }

        public async Task<Service> CreateServiceAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task UpdateServiceAsync(Service service)
        {
            _context.Entry(service).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                service.IsDeleted = true;
                service.DeletionTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ToggleServiceStatusAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                service.IsActive = !(service.IsActive ?? true);
                service.LastModificationTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalServicesAsync(Guid tenantId)
        {
            return await _context.Services.CountAsync(s => s.TenantId == tenantId && s.IsDeleted != true);
        }

        public async Task<int> GetActiveServicesAsync(Guid tenantId)
        {
            return await _context.Services.CountAsync(s => s.TenantId == tenantId && s.IsDeleted != true && s.IsActive == true);
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
