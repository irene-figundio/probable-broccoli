namespace WorkBotAI.API.Services
{
    public interface IPaymentGatewayService
    {
        Task<(bool Success, string TransactionId, string? Error)> ProcessAsync(decimal amount, string method, string? notes);
    }

    public class PaymentGatewayService : IPaymentGatewayService
    {
        public async Task<(bool Success, string TransactionId, string? Error)> ProcessAsync(decimal amount, string method, string? notes)
        {
            // Integration with PayPal or Nexi would go here.
            // For now, we implement the logic to handle various methods.
            await Task.Delay(500); // Simulate network call

            if (amount <= 0)
                return (false, string.Empty, "Importo non valido");

            // Successful transaction
            return (true, $"TXN_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}", null);
        }
    }
}
