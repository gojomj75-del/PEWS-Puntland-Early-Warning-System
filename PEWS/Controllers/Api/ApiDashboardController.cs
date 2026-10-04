using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models;
using PEWS.Models.Enums;
using PEWS.Services;
using PEWS.ViewModels.Api;
using System.Security.Claims;

namespace PEWS.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ApiDashboardController(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("EmergencyOfficer");

        var reportsQuery = _context.UserReports.AsQueryable();
        if (!isAdmin)
            reportsQuery = reportsQuery.Where(r => r.ReportedById == userId);

        return Ok(new DashboardStatsResponse
        {
            TotalReports        = await reportsQuery.CountAsync(),
            ActiveAlerts        = await _context.Alerts.CountAsync(a => a.Status == AlertStatus.Published && a.IsPublished),
            PendingReports      = await reportsQuery.CountAsync(r => r.Status == ReportStatus.Pending),
            VerifiedReports     = await reportsQuery.CountAsync(r => r.Status == ReportStatus.Verified),
            UnreadNotifications = await _notificationService.GetUnreadCountAsync(userId)
        });
    }
}
