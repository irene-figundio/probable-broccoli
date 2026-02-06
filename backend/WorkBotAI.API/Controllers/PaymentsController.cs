using Microsoft.AspNetCore.Mvc;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentRepository paymentRepository, ILogger<PaymentsController> logger)
    {
        _paymentRepository = paymentRepository;
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
            _logger.LogError(ex, "Errore nel recupero dei pagamenti");
            return StatusCode(500, new { success = false, error = "DATABASE_ERROR" });
        }
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentResponseDto>> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        try
        {
            _logger.LogInformation("Processing {Method} payment for subscription {Id}", dto.PaymentMethod, dto.SubscriptionId);

            // Simula chiamata a gateway di pagamento (PayPal/Nexi)
            bool isSuccess = true; // In una vera implementazione qui ci sarebbe la logica del gateway
            string transactionId = Guid.NewGuid().ToString();

            if (isSuccess)
            {
                var payment = new Payment
                {
                    SubscriptionId = dto.SubscriptionId,
                    DatePayment = DateTime.UtcNow,
                    ImportValue = dto.Amount,
                    IvaValue = dto.Amount * 0.22m, // Esempio IVA 22%
                    StatusPaymentId = 1, // Supponiamo 1 = Success/Completed
                    Notes = $"{dto.PaymentMethod} Transaction: {transactionId}. {dto.Notes}",
                    PaymentTypeId = dto.PaymentMethod.ToLower() == "paypal" ? 1 : 2 // Esempio mapping
                };

                await _paymentRepository.CreatePaymentAsync(payment);

                return Ok(new PaymentResponseDto
                {
                    Success = true,
                    TransactionId = transactionId,
                    RedirectUrl = "https://workbotai.com/payment/success"
                });
            }

            return BadRequest(new PaymentResponseDto
            {
                Success = false,
                Error = "Il pagamento è stato rifiutato dal gateway"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'elaborazione del pagamento");
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
        var types = await _paymentRepository.GetPaymentTypesAsync();
        return Ok(new { success = true, data = types.Select(t => new { t.Id, t.Name }) });
    }

    [HttpGet("statuses")]
    public async Task<ActionResult> GetPaymentStatuses()
    {
        var statuses = await _paymentRepository.GetPaymentStatusesAsync();
        return Ok(new { success = true, data = statuses.Select(s => new { s.Id, s.Name }) });
    }
}
