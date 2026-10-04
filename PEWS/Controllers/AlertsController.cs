using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models;
using PEWS.Models.Enums;
using PEWS.Services;
using PEWS.ViewModels;

namespace PEWS.Controllers;

[Authorize]
public class AlertsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public AlertsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        IAuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(string? search, AlertStatus? status, SeverityLevel? severity, int? categoryId, int page = 1)
    {
        const int pageSize = 10;

        var query = _context.Alerts
            .Include(a => a.AlertCategory)
            .Include(a => a.Region)
            .Include(a => a.CreatedBy)
            .AsQueryable();

        // Citizens see only published alerts
        if (User.IsInRole("Citizen"))
            query = query.Where(a => a.Status == AlertStatus.Published && a.IsPublished);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Title.Contains(search) || a.Message.Contains(search));

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (severity.HasValue)
            query = query.Where(a => a.Severity == severity.Value);

        if (categoryId.HasValue)
            query = query.Where(a => a.AlertCategoryId == categoryId.Value);

        var totalCount = await query.CountAsync();
        var alerts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Severity = severity;
        ViewBag.CategoryId = categoryId;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        ViewBag.Categories = await _context.AlertCategories.Where(c => c.IsActive).ToListAsync();

        ViewData["ActivePage"] = "Alerts";
        ViewData["PageTitle"] = "Disaster Alerts";
        return View(alerts);
    }

    public async Task<IActionResult> Details(int id)
    {
        var alert = await _context.Alerts
            .Include(a => a.AlertCategory)
            .Include(a => a.Region)
            .Include(a => a.District)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (alert == null) return NotFound();

        // Increment view count
        alert.ViewCount++;
        await _context.SaveChangesAsync();

        ViewData["ActivePage"] = "Alerts";
        ViewData["PageTitle"] = "Alert Details";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/Alerts\">Alerts</a></li><li class=\"breadcrumb-item active\">Details</li>";
        return View(alert);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
    public async Task<IActionResult> Create()
    {
        var vm = new AlertViewModel();
        await PopulateDropdowns(vm);
        ViewData["ActivePage"] = "Alerts";
        ViewData["PageTitle"] = "Create Alert";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/Alerts\">Alerts</a></li><li class=\"breadcrumb-item active\">Create</li>";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
    public async Task<IActionResult> Create(AlertViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(vm);
            return View(vm);
        }

        var userId = _userManager.GetUserId(User)!;
        var alert = new Alert
        {
            Title = vm.Title,
            Message = vm.Message,
            AlertCategoryId = vm.AlertCategoryId,
            Severity = vm.Severity,
            RegionId = vm.RegionId,
            DistrictId = vm.DistrictId,
            ExpiresAt = vm.ExpiresAt?.ToUniversalTime(),
            CreatedById = userId,
            Status = AlertStatus.Draft,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync(userId, "Alert Created", "Alert", alert.Id.ToString());

        TempData["Success"] = "Alert created. Publish it when ready.";
        return RedirectToAction(nameof(Details), new { id = alert.Id });
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
    public async Task<IActionResult> Edit(int id)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert == null) return NotFound();

        var vm = new AlertViewModel
        {
            Id = alert.Id,
            Title = alert.Title,
            Message = alert.Message,
            AlertCategoryId = alert.AlertCategoryId,
            Severity = alert.Severity,
            RegionId = alert.RegionId,
            DistrictId = alert.DistrictId,
            ExpiresAt = alert.ExpiresAt,
            IsPublished = alert.IsPublished
        };

        await PopulateDropdowns(vm);
        ViewData["ActivePage"] = "Alerts";
        ViewData["PageTitle"] = "Edit Alert";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/Alerts\">Alerts</a></li><li class=\"breadcrumb-item active\">Edit</li>";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
    public async Task<IActionResult> Edit(int id, AlertViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(vm);
            return View(vm);
        }

        var alert = await _context.Alerts.FindAsync(id);
        if (alert == null) return NotFound();

        alert.Title = vm.Title;
        alert.Message = vm.Message;
        alert.AlertCategoryId = vm.AlertCategoryId;
        alert.Severity = vm.Severity;
        alert.RegionId = vm.RegionId;
        alert.DistrictId = vm.DistrictId;
        alert.ExpiresAt = vm.ExpiresAt?.ToUniversalTime();
        alert.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(_userManager.GetUserId(User), "Alert Updated", "Alert", id.ToString());

        TempData["Success"] = "Alert updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
    public async Task<IActionResult> Publish(int id)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert == null) return NotFound();

        alert.Status = AlertStatus.Published;
        alert.IsPublished = true;
        alert.PublishedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Broadcast notification to all users
        await _notificationService.BroadcastAsync(
            $"ALERT: {alert.Title}",
            alert.Message.Length > 200 ? alert.Message[..200] + "..." : alert.Message,
            NotificationType.DisasterAlert, id, "Alert");

        await _auditService.LogAsync(_userManager.GetUserId(User), "Alert Published", "Alert", id.ToString());

        TempData["Success"] = "Alert published and all users notified.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Expire(int id)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert == null) return NotFound();

        alert.Status = AlertStatus.Expired;
        alert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Alert marked as expired.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert == null) return NotFound();

        _context.Alerts.Remove(alert);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync(_userManager.GetUserId(User), "Alert Deleted", "Alert", id.ToString());

        TempData["Success"] = "Alert deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(AlertViewModel vm)
    {
        vm.AlertCategories = new SelectList(
            await _context.AlertCategories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", vm.AlertCategoryId);
        vm.Regions = new SelectList(
            await _context.Regions.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync(), "Id", "Name", vm.RegionId);
        vm.Districts = vm.RegionId.HasValue
            ? new SelectList(await _context.Districts.Where(d => d.RegionId == vm.RegionId && d.IsActive).ToListAsync(), "Id", "Name", vm.DistrictId)
            : new SelectList(Enumerable.Empty<object>(), "Id", "Name");
    }
}
