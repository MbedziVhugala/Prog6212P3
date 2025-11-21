using System.Collections.Generic;

namespace Prog6212.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardViewModel()
        {
            Stats = new DashboardStats();
        }

        public string UserName { get; set; }
        public string UserRole { get; set; }
        public List<ClaimSummaryViewModel> RecentClaims { get; set; } = new List<ClaimSummaryViewModel>();
        public DashboardStats Stats { get; set; }
        public List<ClaimSummaryViewModel> PendingApprovals { get; set; } = new List<ClaimSummaryViewModel>();
    }

    public class DashboardStats
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmountApproved { get; set; }
        public int TotalUsers { get; set; }
    }
}