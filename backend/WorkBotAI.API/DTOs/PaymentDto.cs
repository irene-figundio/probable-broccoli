using System;

namespace WorkBotAI.API.DTOs
{
    public class PaymentListDto
    {
        public int Id { get; set; }
        public int? SubscriptionId { get; set; }
        public string? TenantName { get; set; }
        public DateTime? DatePayment { get; set; }
        public int? PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }
        public decimal? ImportValue { get; set; }
        public decimal? IvaValue { get; set; }
        public int? StatusPaymentId { get; set; }
        public string? StatusPaymentName { get; set; }
    }

    public class ProcessPaymentDto
    {
        public int SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "PayPal"; // PayPal, Nexi
        public string? Notes { get; set; }
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? Error { get; set; }
        public string? RedirectUrl { get; set; }
    }
}
