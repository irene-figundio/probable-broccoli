using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class RegisterRepository : IRegisterRepository
    {
        private readonly WorkBotAIContext _context;

        public RegisterRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Mail == email && u.IsDeleted != true);
        }

        public async Task<RegisterResponseDto> RegisterTenantAsync(RegisterTenantDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tenant = new Tenant { Id = Guid.NewGuid(), Name = dto.BusinessName, CategoryId = dto.CategoryId, IsActive = true, CreationDate = DateTime.UtcNow, IsDeleted = false };
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                var subscription = new Subscription { TenantId = tenant.Id, PlaneId = 1, StatusId = 1, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)) };
                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                var user = new User { TenantId = tenant.Id, FirstName = dto.OwnerFirstName, LastName = dto.OwnerLastName, Mail = dto.OwnerEmail, UserName = dto.OwnerEmail, Password = dto.OwnerPassword, RoleId = 2, IsActive = true, IsSuperAdmin = false, CreationTime = DateTime.UtcNow, IsDeleted = false };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new RegisterResponseDto
                {
                    Success = true,
                    TenantId = tenant.Id,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Mail ?? string.Empty,
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Role = "Owner",
                        IsSuperAdmin = false,
                        TenantId = tenant.Id.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new RegisterResponseDto { Success = false, Error = $"Errore durante la registrazione: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
