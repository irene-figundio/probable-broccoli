using Microsoft.AspNetCore.Mvc;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Services;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayService _gatewayService;
    private readonly IAuditService _auditService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentRepository paymentRepository,
        IPaymentGatewayService gatewayService,
        IAuditService auditService,
        ILogger<PaymentsController> logger)
    {
        _paymentRepository = paymentRepository;
        _gatewayService = gatewayService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetPayments([FromQuery] Guid? tenantId, [FromQuery] int? statusId)
    {
        try
        {
            var payments = await _paymentRepository.GetPaymentsAsync(tenantId, statusId);
            var dtos = payments.Select(p => new PaymentListDto
            {
                Id = p.Id,
                SubscriptionId = p.SubscriptionId,
                TenantName = p.Subscription?.Tenant?.Name,
                DatePayment = p.DatePayment,
                PaymentTypeId = p.PaymentTypeId,
                PaymentTypeName = p.PaymentType?.Name,
                ImportValue = p.ImportValue,
                IvaValue = p.IvaValue,
                StatusPaymentId = p.StatusPaymentId,
                StatusPaymentName = p.StatusPayment?.Name
            });

            return Ok(new { success = true, data = dtos });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Payments", "Error retrieving payments", ex);
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentResponseDto>> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        try
        {
            _logger.LogInformation("Processing {Method} payment for subscription {Id}", dto.PaymentMethod, dto.SubscriptionId);

            var (isSuccess, transactionId, error) = await _gatewayService.ProcessAsync(dto.Amount, dto.PaymentMethod, dto.Notes);

            if (isSuccess)
            {
                var payment = new Payment
                {
                    SubscriptionId = dto.SubscriptionId,
                    DatePayment = DateTime.UtcNow,
                    ImportValue = dto.Amount,
                    IvaValue = dto.Amount * 0.22m, // Esempio IVA 22%
                    StatusPaymentId = 2, // 2 = Completed (base on Seed.sql)
                    Notes = $"{dto.PaymentMethod} Transaction: {transactionId}. {dto.Notes}",
                    PaymentTypeId = dto.PaymentMethod.ToLower() == "paypal" ? 3 : 2 // Mapping (base on Seed.sql: 2=CreditCard, 3=PayPal)
                };

                await _paymentRepository.CreatePaymentAsync(payment);
                await _auditService.LogActionAsync("Payments", "ProcessPayment", $"Payment processed successfully: {transactionId} for subscription {dto.SubscriptionId}");

                return Ok(new PaymentResponseDto
                {
                    Success = true,
                    TransactionId = transactionId,
                    RedirectUrl = "https://workbotai.com/payment/success"
                });
            }

            await _auditService.LogActionAsync("Payments", "ProcessPayment", $"Payment rejected: {error}", $"SubID: {dto.SubscriptionId}");
            return BadRequest(new PaymentResponseDto
            {
                Success = false,
                Error = error ?? "Il pagamento è stato rifiutato dal gateway"
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Payments", "Unexpected error during payment processing", ex);
            return StatusCode(500, new PaymentResponseDto
            {
                Success = false,
                Error = "Errore interno durante l'elaborazione del pagamento"
            });
        }
    }

    [HttpGet("types")]
    public async Task<ActionResult> GetPaymentTypes()
    {
        try
        {
            var types = await _paymentRepository.GetPaymentTypesAsync();
            return Ok(new { success = true, data = types.Select(t => new { t.Id, t.Name }) });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Payments", "Error retrieving payment types", ex);
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    [HttpGet("statuses")]
    public async Task<ActionResult> GetPaymentStatuses()
    {
        try
        {
            var statuses = await _paymentRepository.GetPaymentStatusesAsync();
            return Ok(new { success = true, data = statuses.Select(s => new { s.Id, s.Name }) });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Payments", "Error retrieving payment statuses", ex);
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }
}
