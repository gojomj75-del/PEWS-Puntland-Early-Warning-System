using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models.Enums;
using PEWS.ViewModels.Dashboard;

namespace PEWS.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // "Reports" on this dashboard means citizen-submitted UserReports — the actionable
        // incoming stream. DisasterReports is the admin's separate official record (see
        // TotalDisasterRecords) and isn't mixed into these citizen-report analytics.
        var vm = new DashboardViewModel
        {
            TotalUsers = await _context.Users.CountAsync(u => u.IsActive),
            TotalReports = await _context.UserReports.CountAsync(),
            ActiveAlerts = await _context.Alerts.CountAsync(a => a.Status == AlertStatus.Published && a.IsPublished),
            PendingReports = await _context.UserReports.CountAsync(r => r.Status == ReportStatus.Pending),
            VerifiedReports = await _context.UserReports.CountAsync(r => r.IsVerified),
            HighRiskAreas = await _context.Regions.CountAsync(r => r.RiskLevel >= RiskLevel.High),
            TotalRegions = await _context.Regions.CountAsync(r => r.IsActive),
            TotalDisasterRecords = await _context.DisasterReports.CountAsync(),

            RecentReports = await _context.UserReports
                .Include(r => r.DisasterType)
                .Include(r => r.ReportedBy)
                .Include(r => r.Region)
                .OrderByDescending(r => r.CreatedAt)
                .Take(8)
                .ToListAsync(),

            ActiveAlertsList = await _context.Alerts
                .Include(a => a.AlertCategory)
                .Include(a => a.Region)
                .Where(a => a.Status == AlertStatus.Published && a.IsPublished)
                .OrderByDescending(a => a.PublishedAt)
                .Take(5)
                .ToListAsync(),

            ReportsByType = await _context.UserReports
                .Include(r => r.DisasterType)
                .GroupBy(r => r.DisasterType.Name)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count),

            ReportsBySeverity = await _context.UserReports
                .GroupBy(r => r.Severity)
                .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count),

            ReportsByMonth = GetLast12MonthsData(
                await _context.UserReports
                    .Where(r => r.CreatedAt >= DateTime.UtcNow.AddMonths(-11))
                    .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .ToListAsync()),

            ReportsByRegion = await _context.UserReports
                .Where(r => r.Region != null)
                .Include(r => r.Region)
                .GroupBy(r => r.Region!.Name)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count)
        };

        ViewData["ActivePage"] = "Dashboard";
        ViewData["PageTitle"] = "Dashboard";
        return View(vm);
    }

    private static Dictionary<string, int> GetLast12MonthsData(
        IEnumerable<dynamic> rawData)
    {
        var result = new Dictionary<string, int>();
        var now = DateTime.UtcNow;
        for (int i = 11; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            var key = date.ToString("MMM yyyy");
            result[key] = 0;
        }
        foreach (var item in rawData)
        {
            var key = new DateTime(item.Year, item.Month, 1).ToString("MMM yyyy");
            if (result.ContainsKey(key))
                result[key] = item.Count;
        }
        return result;
    }
}
