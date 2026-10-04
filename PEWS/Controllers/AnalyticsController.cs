using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models.Enums;

namespace PEWS.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class AnalyticsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AnalyticsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int year = 0)
    {
        if (year == 0) year = DateTime.UtcNow.Year;

        var from = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddYears(1);

        var reportsByMonth = Enumerable.Range(1, 12).ToDictionary(
            m => new DateTime(year, m, 1).ToString("MMM"), m => 0);

        var raw = await _context.UserReports
            .Where(r => r.CreatedAt >= from && r.CreatedAt < to)
            .GroupBy(r => r.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var item in raw)
            reportsByMonth[new DateTime(year, item.Month, 1).ToString("MMM")] = item.Count;

        var totalReports = await _context.UserReports.CountAsync(r => r.CreatedAt >= from && r.CreatedAt < to);
        var totalAlerts = await _context.Alerts.CountAsync(a => a.CreatedAt >= from && a.CreatedAt < to);
        var verifiedReports = await _context.UserReports.CountAsync(r => r.IsVerified && r.CreatedAt >= from && r.CreatedAt < to);
        var criticalReports = await _context.UserReports.CountAsync(r => r.Severity == SeverityLevel.Critical && r.CreatedAt >= from && r.CreatedAt < to);

        var byType = await _context.UserReports
            .Where(r => r.CreatedAt >= from && r.CreatedAt < to)
            .Include(r => r.DisasterType)
            .GroupBy(r => r.DisasterType.Name)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var byRegion = await _context.UserReports
            .Where(r => r.CreatedAt >= from && r.CreatedAt < to && r.Region != null)
            .Include(r => r.Region)
            .GroupBy(r => r.Region!.Name)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var bySeverity = await _context.UserReports
            .Where(r => r.CreatedAt >= from && r.CreatedAt < to)
            .GroupBy(r => r.Severity)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        ViewBag.Year = year;
        ViewBag.TotalReports = totalReports;
        ViewBag.TotalAlerts = totalAlerts;
        ViewBag.VerifiedReports = verifiedReports;
        ViewBag.CriticalReports = criticalReports;
        ViewBag.ReportsByMonth = reportsByMonth;
        ViewBag.ByType = byType;
        ViewBag.ByRegion = byRegion;
        ViewBag.BySeverity = bySeverity;
        ViewBag.Years = Enumerable.Range(DateTime.UtcNow.Year - 3, 5).Reverse().ToList();

        ViewData["ActivePage"] = "Analytics";
        ViewData["PageTitle"] = "Analytics";
        return View();
    }
}
