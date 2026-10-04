using PEWS.Models;
using PEWS.Models.Enums;

namespace PEWS.ViewModels.Dashboard;

public class DashboardViewModel
{
    // Summary stats
    public int TotalUsers { get; set; }
    public int TotalReports { get; set; }
    public int ActiveAlerts { get; set; }
    public int PendingReports { get; set; }
    public int HighRiskAreas { get; set; }
    public int VerifiedReports { get; set; }
    public int TotalRegions { get; set; }
    public int TotalDisasterRecords { get; set; }

    // Recent data — citizen-submitted reports (see Models/UserReport.cs)
    public IEnumerable<UserReport> RecentReports { get; set; } = new List<UserReport>();
    public IEnumerable<Alert> ActiveAlertsList { get; set; } = new List<Alert>();

    // Chart data
    public Dictionary<string, int> ReportsByType { get; set; } = new();
    public Dictionary<string, int> ReportsBySeverity { get; set; } = new();
    public Dictionary<string, int> ReportsByMonth { get; set; } = new();
    public Dictionary<string, int> AlertsByCategory { get; set; } = new();
    public Dictionary<string, int> ReportsByRegion { get; set; } = new();
}
