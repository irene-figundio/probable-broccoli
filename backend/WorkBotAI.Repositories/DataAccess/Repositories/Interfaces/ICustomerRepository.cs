using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerListDto>> GetCustomersAsync(Guid? tenantId, string search);
        Task<Customer> GetCustomerByIdAsync(int id);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(int id);
        Task<int> GetTotalCustomersAsync(Guid tenantId);
        Task<int> GetNewCustomersThisMonthAsync(Guid tenantId);
        Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(int customerId);
    }
}
