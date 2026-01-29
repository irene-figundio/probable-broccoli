using Microsoft.AspNetCore.Mvc;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    public PaymentsController()
    {
    }

    [HttpPost("paypal/create")]
    public IActionResult CreatePayPalPayment([FromBody] PaymentRequest request)
    {
        // Esempio di logica per creare un pagamento PayPal
        // In un caso reale, qui si chiamerebbero le API di PayPal
        return Ok(new
        {
            PaymentId = "PAYID-" + Guid.NewGuid().ToString("N").ToUpper(),
            ApprovalUrl = "https://www.paypal.com/checkoutnow?token=EC-" + Guid.NewGuid().ToString("N").ToUpper(),
            Status = "Created"
        });
    }

    [HttpPost("paypal/capture")]
    public IActionResult CapturePayPalPayment([FromBody] string paymentId)
    {
        // Esempio di logica per catturare un pagamento PayPal
        return Ok(new
        {
            Status = "Completed",
            TransactionId = Guid.NewGuid().ToString("N"),
            Message = "Pagamento PayPal completato con successo"
        });
    }

    [HttpPost("nexi/pay")]
    public IActionResult NexiPayment([FromBody] NexiPaymentRequest request)
    {
        // Esempio di logica per un pagamento Nexi (XPay)
        // In un caso reale, si genererebbe il MAC (firma) e si reindirizzerebbe l'utente
        return Ok(new
        {
            Status = "Redirect",
            PaymentUrl = "https://ecommerce.nexi.it/ecomm/ecomm/DispatcherServlet",
            Parameters = new
            {
                alias = "PAY_12345",
                importo = request.Amount * 100, // In centesimi
                divisa = "EUR",
                codTrans = "TRANS-" + DateTime.Now.Ticks,
                urlMessaggi = "https://api.workbotai.com/api/payments/nexi/callback"
            }
        });
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Description { get; set; } = string.Empty;
    }

    public class NexiPaymentRequest
    {
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
    }
}
