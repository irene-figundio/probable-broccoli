using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class StaffRepository : IStaffRepository
    {
        private readonly WorkBotAIContext _context;

        public StaffRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StaffListDto>> GetStaffAsync(Guid? tenantId, string search, int? jobTypeId)
        {
            var query = _context.Staff
                .Where(s => s.IsDeleted != true)
                .Include(s => s.Tenant)
                .Include(s => s.JobType)
                .Select(s => new StaffListDto
                {
                    Id = s.Id,
                    TenantId = s.TenantId,
                    TenantName = s.Tenant != null ? s.Tenant.Name : null,
                    Name = s.Name,
                    JobTypeId = s.JobTypeId,
                    JobTypeName = s.JobType != null ? s.JobType.Name : null,
                    IsActive = s.IsActive ?? true,
                    CreationTime = s.CreationTime,
                    AppointmentsCount = s.Appointments.Count(a => a.IsDeleted != true)
                });

            if (tenantId.HasValue) query = query.Where(s => s.TenantId == tenantId.Value);
            if (jobTypeId.HasValue) query = query.Where(s => s.JobTypeId == jobTypeId.Value);
            if (!string.IsNullOrEmpty(search)) query = query.Where(s => s.Name.Contains(search));

            return await query.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<Staff> GetStaffByIdAsync(int id)
        {
            return await _context.Staff
                .Where(s => s.Id == id && s.IsDeleted != true)
                .Include(s => s.Tenant)
                .Include(s => s.JobType)
                .FirstOrDefaultAsync();
        }

        public async Task<Staff> CreateStaffAsync(Staff staff)
        {
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        public async Task UpdateStaffAsync(Staff staff)
        {
            _context.Entry(staff).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStaffAsync(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                staff.IsDeleted = true;
                staff.DeletionTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ToggleStaffStatusAsync(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                staff.IsActive = !(staff.IsActive ?? true);
                staff.LastModificationTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalStaffAsync(Guid tenantId)
        {
            return await _context.Staff.CountAsync(s => s.TenantId == tenantId && s.IsDeleted != true);
        }

        public async Task<int> GetActiveStaffAsync(Guid tenantId)
        {
            return await _context.Staff.CountAsync(s => s.TenantId == tenantId && s.IsDeleted != true && s.IsActive == true);
        }

        public async Task<IEnumerable<JobType>> GetJobTypesAsync()
        {
            return await _context.JobTypes.OrderBy(j => j.Name).ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(int staffId)
        {
            return await _context.Appointments
                .Where(a => a.StaffId == staffId && a.IsDeleted != true)
                .Include(a => a.Customer)
                .Include(a => a.Status)
                .OrderByDescending(a => a.StartTime)
                .Take(10)
                .ToListAsync();
        }
    }
}
