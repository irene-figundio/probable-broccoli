using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly WorkBotAIContext _context;

        public UserRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsersAsync(Guid? tenantId)
        {
            var query = _context.Users
                .Where(u => u.IsDeleted != true)
                .Include(u => u.Role)
                .Include(u => u.Tenant)
                .AsQueryable();

            if (tenantId.HasValue)
            {
                query = query.Where(u => u.TenantId == tenantId.Value);
            }

            return await query.OrderByDescending(u => u.CreationTime).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.Id == id && u.IsDeleted != true)
                .Include(u => u.Role)
                .Include(u => u.Status)
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Mail == email && u.IsDeleted != true);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username && u.IsDeleted != true);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsDeleted = true;
                user.DeletionTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive == true)
                .ToListAsync();
        }
    }
}
