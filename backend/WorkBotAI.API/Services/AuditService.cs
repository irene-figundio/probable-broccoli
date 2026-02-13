using System.Security.Claims;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string source, string action, string message, string? context = null, Guid? tenantId = null, int? userId = null);
        Task LogErrorAsync(string source, string message, Exception? ex = null, Guid? tenantId = null, int? userId = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ISystemLogRepository _logRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ISystemLogRepository logRepository, IHttpContextAccessor httpContextAccessor)
        {
            _logRepository = logRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(string source, string action, string message, string? context = null, Guid? tenantId = null, int? userId = null)
        {
            await CreateLogEntryAsync("info", source, $"[{action}] {message}", context, tenantId, userId);
        }

        public async Task LogErrorAsync(string source, string message, Exception? ex = null, Guid? tenantId = null, int? userId = null)
        {
            var fullMessage = ex != null ? $"{message} | Exception: {ex.Message} | Stack: {ex.StackTrace}" : message;
            await CreateLogEntryAsync("error", source, fullMessage, null, tenantId, userId);
        }

        private async Task CreateLogEntryAsync(string level, string source, string message, string? context, Guid? tenantId, int? userId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            string? userRole = null;
            string? userEmail = null;

            if (httpContext?.User != null)
            {
                // Try to extract IDs and info from claims
                if (!tenantId.HasValue)
                {
                    var tenantClaim = httpContext.User.FindFirst("TenantId")?.Value;
                    if (Guid.TryParse(tenantClaim, out var tId)) tenantId = tId;
                }

                if (!userId.HasValue)
                {
                    var userClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userClaim, out var uId)) userId = uId;
                }

                userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            }

            // Enrich context with technical details and user info
            var technicalContext = new
            {
                Method = httpContext?.Request?.Method,
                Path = httpContext?.Request?.Path.Value,
                Role = userRole,
                Email = userEmail,
                CustomContext = context
            };

            var enrichedContext = System.Text.Json.JsonSerializer.Serialize(technicalContext);

            var log = new SystemLog
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Source = source,
                Message = message,
                Context = enrichedContext,
                TenantId = tenantId,
                UserId = userId,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString()
            };

            await _logRepository.CreateLogAsync(log);
        }
    }
}
