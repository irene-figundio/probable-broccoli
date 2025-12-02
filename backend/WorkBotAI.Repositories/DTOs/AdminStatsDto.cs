namespace WorkBotAI.Repositories.DTOs
{
    public class AdminStatsDto
    {
        public TenantStats Tenants { get; set; }
        public UserStats Users { get; set; }
        public RevenueStats Revenue { get; set; }
        public SubscriptionStats Subscriptions { get; set; }
        public AppointmentStats Appointments { get; set; }
        public GrowthStats Growth { get; set; }
    }

    public class TenantStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Suspended { get; set; }
        public int GrowthThisMonth { get; set; }
    }

    public class UserStats
    {
        public int Total { get; set; }
        public int ActiveToday { get; set; }
    }

    public class RevenueStats
    {
        public decimal Mrr { get; set; }
        public decimal Arr { get; set; }
        public decimal GrowthPercentage { get; set; }
    }

    public class SubscriptionStats
    {
        public int Active { get; set; }
        public int Trial { get; set; }
        public PlanDistribution ByPlan { get; set; }
    }

    public class PlanDistribution
    {
        public int Basic { get; set; }
        public int Pro { get; set; }
        public int Enterprise { get; set; }
    }

    public class AppointmentStats
    {
        public int Total { get; set; }
        public int ThisWeek { get; set; }
        public int ThisMonth { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class GrowthStats
    {
        public decimal Percentage { get; set; }
        public string Trend { get; set; }
    }
}
