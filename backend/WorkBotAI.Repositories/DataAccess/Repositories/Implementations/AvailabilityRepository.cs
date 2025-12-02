using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Implementations
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly WorkBotAIContext _context;

        public AvailabilityRepository(WorkBotAIContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Availability>> GetAvailabilitiesAsync(Guid? tenantId, int? staffId, DateTime? from, DateTime? to)
        {
            var query = _context.Availabilities
                .Include(a => a.Staff)
                .Include(a => a.Tenant)
                .AsQueryable();

            if (tenantId.HasValue) query = query.Where(a => a.TenantId == tenantId.Value);
            if (staffId.HasValue) query = query.Where(a => a.StaffId == staffId.Value);
            if (from.HasValue) query = query.Where(a => a.EndTime >= from.Value);
            if (to.HasValue) query = query.Where(a => a.StartTime <= to.Value);

            return await query.OrderBy(a => a.StartTime).ToListAsync();
        }

        public async Task<Availability> GetAvailabilityByIdAsync(int id)
        {
            return await _context.Availabilities
                .Include(a => a.Staff)
                .Include(a => a.Tenant)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Availability> CreateAvailabilityAsync(Availability availability)
        {
            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync();
            return availability;
        }

        public async Task UpdateAvailabilityAsync(Availability availability)
        {
            _context.Entry(availability).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAvailabilityAsync(int id)
        {
            var availability = await _context.Availabilities.FindAsync(id);
            if (availability != null)
            {
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(Guid tenantId, DateTime date, int? serviceId, int? staffId)
        {
            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1);
            var serviceDuration = 30;
            if (serviceId.HasValue)
            {
                var service = await _context.Services.FindAsync(serviceId.Value);
                if (service != null) serviceDuration = service.DurationMin ?? 30;
            }

            var availabilities = await _context.Availabilities.Include(a => a.Staff).Where(a => a.TenantId == tenantId && (!staffId.HasValue || a.StaffId == staffId) && a.StartTime < endOfDay && a.EndTime > startOfDay).ToListAsync();
            var appointments = await _context.Appointments.Where(a => a.TenantId == tenantId && (!staffId.HasValue || a.StaffId == staffId) && a.StartTime < endOfDay && a.EndTime > startOfDay && a.StatusId != 4).ToListAsync();
            var slots = new List<AvailableSlotDto>();

            foreach (var avail in availabilities)
            {
                var currentTime = avail.StartTime < startOfDay ? startOfDay : avail.StartTime;
                var endTime = avail.EndTime > endOfDay ? endOfDay : avail.EndTime;

                while (currentTime.AddMinutes(serviceDuration) <= endTime)
                {
                    var slotEnd = currentTime.AddMinutes(serviceDuration);
                    var isBooked = appointments.Any(app => app.StaffId == avail.StaffId && app.StartTime < slotEnd && app.EndTime > currentTime);

                    if (!isBooked)
                    {
                        slots.Add(new AvailableSlotDto { StaffId = avail.StaffId ?? 0, StaffName = avail.Staff?.Name ?? "Qualsiasi", StartTime = currentTime, EndTime = slotEnd });
                    }
                    currentTime = currentTime.AddMinutes(serviceDuration);
                }
            }
            return slots.OrderBy(s => s.StartTime).ThenBy(s => s.StaffName);
        }

        public async Task<IEnumerable<Availability>> GetWeekAvailabilityAsync(Guid tenantId, DateTime startDate)
        {
            var endDate = startDate.AddDays(7);
            return await _context.Availabilities
                .Include(a => a.Staff)
                .Where(a => a.TenantId == tenantId && a.StartTime < endDate && a.EndTime > startDate)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }
    }
}
