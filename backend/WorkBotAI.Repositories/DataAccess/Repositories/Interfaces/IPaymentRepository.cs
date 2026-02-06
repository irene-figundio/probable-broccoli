using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetPaymentsAsync(Guid? tenantId, int? statusId);
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task<IEnumerable<PaymentType>> GetPaymentTypesAsync();
        Task<IEnumerable<PaymentStatus>> GetPaymentStatusesAsync();
    }
}
