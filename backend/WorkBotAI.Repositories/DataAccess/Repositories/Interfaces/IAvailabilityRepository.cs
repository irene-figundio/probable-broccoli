using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IAvailabilityRepository
    {
        Task<IEnumerable<Availability>> GetAvailabilitiesAsync(Guid? tenantId, int? staffId, DateTime? from, DateTime? to);
        Task<Availability> GetAvailabilityByIdAsync(int id);
        Task<Availability> CreateAvailabilityAsync(Availability availability);
        Task UpdateAvailabilityAsync(Availability availability);
        Task DeleteAvailabilityAsync(int id);
        Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(Guid tenantId, DateTime date, int? serviceId, int? staffId);
        Task<IEnumerable<Availability>> GetWeekAvailabilityAsync(Guid tenantId, DateTime startDate);
    }
}
