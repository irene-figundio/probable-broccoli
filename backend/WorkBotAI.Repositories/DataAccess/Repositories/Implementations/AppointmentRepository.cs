using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly WorkBotAIContext _context;

        public AppointmentRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync(Guid? tenantId, int? statusId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Appointments
                .Where(a => a.IsDeleted != true)
                .Include(a => a.Tenant)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Include(a => a.Status)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asvc => asvc.Service)
                .AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(a => a.TenantId == tenantId.Value);

            if (statusId.HasValue)
                query = query.Where(a => a.StatusId == statusId.Value);

            if (fromDate.HasValue)
                query = query.Where(a => a.StartTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.StartTime <= toDate.Value);

            return await query.OrderByDescending(a => a.StartTime).ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments
                .Where(a => a.Id == id && a.IsDeleted != true)
                .Include(a => a.Tenant)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Include(a => a.Resource)
                .Include(a => a.Status)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asvc => asvc.Service)
                .FirstOrDefaultAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Entry(appointment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.IsDeleted = true;
                appointment.DeletionTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ChangeAppointmentStatusAsync(int id, int statusId)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.StatusId = statusId;
                appointment.LastModificationTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<AppointmentStatus>> GetAppointmentStatusesAsync()
        {
            return await _context.AppointmentStatuses
                .Where(s => s.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync(Guid? tenantId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var query = _context.Appointments
                .Where(a => a.IsDeleted != true && a.StartTime >= today && a.StartTime < tomorrow)
                .Include(a => a.Tenant)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Include(a => a.Status)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asvc => asvc.Service)
                .AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(a => a.TenantId == tenantId.Value);

            return await query.OrderBy(a => a.StartTime).ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetWeekAppointmentsAsync(Guid? tenantId)
        {
            var today = DateTime.Today;
            var endOfWeek = today.AddDays(7);

            var query = _context.Appointments
                .Where(a => a.IsDeleted != true && a.StartTime >= today && a.StartTime < endOfWeek)
                .Include(a => a.Tenant)
                .Include(a => a.Customer)
                .Include(a => a.Staff)
                .Include(a => a.Status)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asvc => asvc.Service)
                .AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(a => a.TenantId == tenantId.Value);

            return await query.OrderBy(a => a.StartTime).ToListAsync();
        }
    }
}
