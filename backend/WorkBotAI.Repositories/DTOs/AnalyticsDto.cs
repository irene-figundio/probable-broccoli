using System;
using System.Collections.Generic;

namespace WorkBotAI.Repositories.DTOs
{
    public class TenantAnalyticsDto
    {
        public string Period { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public AnalyticsSummary Summary { get; set; }
        public Dictionary<string, int> AppointmentsByDayOfWeek { get; set; }
        public List<object> AppointmentsTrend { get; set; }
        public List<object> TopServices { get; set; }
        public List<object> TopCustomers { get; set; }
        public List<object> StaffPerformance { get; set; }
    }

    public class AnalyticsSummary
    {
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public double CompletionRate { get; set; }
        public double CancellationRate { get; set; }
        public double AvgAppointmentsPerDay { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
    }

    public class PeriodComparisonDto
    {
        public ComparisonItem Appointments { get; set; }
        public ComparisonItem NewCustomers { get; set; }
    }

    public class ComparisonItem
    {
        public int ThisMonth { get; set; }
        public int LastMonth { get; set; }
        public double ChangePercent { get; set; }
        public string Trend { get; set; }
    }
}
