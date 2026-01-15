using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAppointmentsAsync(Guid? tenantId, int? statusId, DateTime? fromDate, DateTime? toDate);
        Task<Appointment> GetAppointmentByIdAsync(int id);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task DeleteAppointmentAsync(int id);
        Task ChangeAppointmentStatusAsync(int id, int statusId);
        Task<IEnumerable<AppointmentStatus>> GetAppointmentStatusesAsync();
        Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync(Guid? tenantId);
        Task<IEnumerable<Appointment>> GetWeekAppointmentsAsync(Guid? tenantId);
    }
}
