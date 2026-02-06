using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class PaymentRepository : Interfaces.IPaymentRepository
    {
        private readonly WorkBotAIContext _context;

        public PaymentRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsAsync(Guid? tenantId, int? statusId)
        {
            var query = _context.Payments
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Tenant)
                .Include(p => p.PaymentType)
                .Include(p => p.StatusPayment)
                .AsQueryable();

            if (tenantId.HasValue)
            {
                query = query.Where(p => p.Subscription != null && p.Subscription.TenantId == tenantId.Value);
            }

            if (statusId.HasValue)
            {
                query = query.Where(p => p.StatusPaymentId == statusId.Value);
            }

            return await query.OrderByDescending(p => p.DatePayment).ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Subscription)
                .Include(p => p.PaymentType)
                .Include(p => p.StatusPayment)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PaymentType>> GetPaymentTypesAsync()
        {
            return await _context.PaymentTypes.ToListAsync();
        }

        public async Task<IEnumerable<PaymentStatus>> GetPaymentStatusesAsync()
        {
            return await _context.PaymentStatuses.ToListAsync();
        }
    }
}
