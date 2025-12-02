using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<IEnumerable<StaffListDto>> GetStaffAsync(Guid? tenantId, string search, int? jobTypeId);
        Task<Staff> GetStaffByIdAsync(int id);
        Task<Staff> CreateStaffAsync(Staff staff);
        Task UpdateStaffAsync(Staff staff);
        Task DeleteStaffAsync(int id);
        Task ToggleStaffStatusAsync(int id);
        Task<int> GetTotalStaffAsync(Guid tenantId);
        Task<int> GetActiveStaffAsync(Guid tenantId);
        Task<IEnumerable<JobType>> GetJobTypesAsync();
        Task<IEnumerable<Appointment>> GetRecentAppointmentsAsync(int staffId);
    }
}
