using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly WorkBotAIContext _context;

        public CustomerRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerListDto>> GetCustomersAsync(Guid? tenantId, string search)
        {
            var query = _context.Customers
                .Where(c => c.IsDeleted != true)
                .Include(c => c.Tenant)
                .Select(c => new CustomerListDto
                {
                    Id = c.Id,
                    TenantId = c.TenantId,
                    TenantName = c.Tenant != null ? c.Tenant.Name : null,
                    FullName = c.FullName,
                    Phone = c.Phone,
                    Email = c.Email,
                    Note = c.Note,
                    CreationTime = c.CreationTime,
                    AppointmentsCount = c.Appointments.Count(a => a.IsDeleted != true)
                });

            if (tenantId.HasValue)
                query = query.Where(c => c.TenantId == tenantId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => (c.FullName != null && c.FullName.Contains(search)) || (c.Phone != null && c.Phone.Contains(search)) || (c.Email != null && c.Email.Contains(search)));

            return await query.OrderBy(c => c.FullName).ToListAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Where(c => c.Id == id && c.IsDeleted != true)
                .Include(c => c.Tenant)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.IsDeleted = true;
                customer.DeletionTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalCustomersAsync(Guid tenantId)
        {
            return await _context.Customers.CountAsync(c => c.TenantId == tenantId && c.IsDeleted != true);
        }

        public async Task<int> GetNewCustomersThisMonthAsync(Guid tenantId)
        {
            var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await _context.Customers.CountAsync(c => c.TenantId == tenantId && c.IsDeleted != true && c.CreationTime >= thisMonth);
        }

        public async Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(int customerId)
        {
            return await _context.Appointments
                .Where(a => a.CustomerId == customerId && a.IsDeleted != true)
                .Include(a => a.Staff)
                .Include(a => a.Status)
                .OrderByDescending(a => a.StartTime)
                .Take(10)
                .ToListAsync();
        }
    }
}
