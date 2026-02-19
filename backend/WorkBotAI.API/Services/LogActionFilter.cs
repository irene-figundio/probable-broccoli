using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.Text.Json;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace WorkBotAI.API.Services
{
    public class LogActionFilter : IAsyncActionFilter
    {
        private readonly ISystemLogRepository _logRepository;

        public LogActionFilter(ISystemLogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Salta il log per le chiamate interne per evitare duplicati
            if (context.HttpContext.Request.Path.Value != null &&
                context.HttpContext.Request.Path.Value.Contains("/internal/"))
            {
                await next();
                return;
            }

            // Abilita il buffering per poter leggere il body più volte
            context.HttpContext.Request.EnableBuffering();

            string requestBody = string.Empty;
            if (context.HttpContext.Request.Body.CanRead)
            {
                using (var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.HttpContext.Request.Body.Position = 0;
                }
            }

            // Esegue l'azione
            var executedContext = await next();

            // Cattura il body della risposta
            string responseBody = string.Empty;
            if (executedContext.Result is ObjectResult objectResult)
            {
                try
                {
                    responseBody = JsonSerializer.Serialize(objectResult.Value);
                }
                catch
                {
                    responseBody = "[Complex or Non-Serializable Response]";
                }
            }
            else if (executedContext.Result is ContentResult contentResult)
            {
                responseBody = contentResult.Content ?? string.Empty;
            }

            // Estrazione User ID e Tenant ID dai Claims
            int? userId = null;
            Guid? tenantId = null;

            var userClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userClaim, out var uId)) userId = uId;

            var tenantClaim = context.HttpContext.User.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(tenantClaim, out var tId)) tenantId = tId;

            // Creazione del log
            var log = new SystemLog
            {
                Timestamp = DateTime.UtcNow,
                Level = "info",
                Source = context.ActionDescriptor.DisplayName ?? "Unknown",
                Message = $"API Request: {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}",
                UserId = userId,
                TenantId = tenantId,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString(),
                RequestJson = requestBody,
                ResponseJson = responseBody
            };

            // Salvataggio asincrono
            await _logRepository.CreateLogAsync(log);
        }
    }
}
