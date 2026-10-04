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

// The admin/staff-authored official disaster record — independent of UserReportsController,
// which is where citizen submissions from the mobile app are reviewed (see Models/UserReport.cs).
[Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
public class DisasterReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public DisasterReportsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IFileUploadService fileUploadService,
        INotificationService notificationService,
        IAuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index(string? search, int? typeId, SeverityLevel? severity,
        ReportStatus? status, int? regionId, int page = 1)
    {
        const int pageSize = 10;

        var query = _context.DisasterReports
            .Include(r => r.DisasterType)
            .Include(r => r.ReportedBy)
            .Include(r => r.Region)
            .Include(r => r.District)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Title.Contains(search) || r.Description.Contains(search) || r.Location!.Contains(search));

        if (typeId.HasValue)
            query = query.Where(r => r.DisasterTypeId == typeId.Value);

        if (severity.HasValue)
            query = query.Where(r => r.Severity == severity.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (regionId.HasValue)
            query = query.Where(r => r.RegionId == regionId.Value);

        var totalCount = await query.CountAsync();
        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.TypeId = typeId;
        ViewBag.Severity = severity;
        ViewBag.Status = status;
        ViewBag.RegionId = regionId;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        ViewBag.DisasterTypes = await _context.DisasterTypes.Where(t => t.IsActive).ToListAsync();
        ViewBag.Regions = await _context.Regions.Where(r => r.IsActive).ToListAsync();

        ViewData["ActivePage"] = "Reports";
        ViewData["PageTitle"] = "Disaster Reports";
        return View(reports);
    }

    public async Task<IActionResult> Details(int id)
    {
        var report = await _context.DisasterReports
            .Include(r => r.DisasterType)
            .Include(r => r.ReportedBy)
            .Include(r => r.VerifiedBy)
            .Include(r => r.Region)
            .Include(r => r.District)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return NotFound();

        ViewData["ActivePage"] = "Reports";
        ViewData["PageTitle"] = "Report Details";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/DisasterReports\">Reports</a></li><li class=\"breadcrumb-item active\">Details</li>";
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new DisasterReportViewModel { IncidentDate = DateTime.Now };
        await PopulateDropdowns(vm);
        ViewData["ActivePage"] = "Reports";
        ViewData["PageTitle"] = "Add Disaster Record";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/DisasterReports\">Reports</a></li><li class=\"breadcrumb-item active\">New Record</li>";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DisasterReportViewModel vm)
    {
        ModelState.Remove("Images");
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(vm);
            return View(vm);
        }

        var userId = _userManager.GetUserId(User)!;

        var report = new DisasterReport
        {
            Title = vm.Title,
            Description = vm.Description,
            DisasterTypeId = vm.DisasterTypeId,
            Severity = vm.Severity,
            RegionId = vm.RegionId,
            DistrictId = vm.DistrictId,
            Location = vm.Location,
            Latitude = vm.Latitude,
            Longitude = vm.Longitude,
            IncidentDate = vm.IncidentDate.ToUniversalTime(),
            ReportedById = userId,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.DisasterReports.Add(report);
        await _context.SaveChangesAsync();

        // Upload images
        if (vm.Images != null)
        {
            foreach (var file in vm.Images.Where(f => f != null && f.Length > 0))
            {
                if (_fileUploadService.IsValidImage(file))
                {
                    var path = await _fileUploadService.UploadImageAsync(file, "reports");
                    if (path != null)
                    {
                        _context.ReportImages.Add(new ReportImage
                        {
                            DisasterReportId = report.Id,
                            ImagePath = path,
                            FileSizeBytes = file.Length,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        await _auditService.LogAsync(userId, "Disaster Report Created", "DisasterReport", report.Id.ToString(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "Disaster record added.";
        return RedirectToAction(nameof(Details), new { id = report.Id });
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
    public async Task<IActionResult> Edit(int id)
    {
        var report = await _context.DisasterReports.Include(r => r.Images).FirstOrDefaultAsync(r => r.Id == id);
        if (report == null) return NotFound();

        var vm = new DisasterReportViewModel
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            DisasterTypeId = report.DisasterTypeId,
            Severity = report.Severity,
            RegionId = report.RegionId,
            DistrictId = report.DistrictId,
            Location = report.Location,
            Latitude = report.Latitude,
            Longitude = report.Longitude,
            IncidentDate = report.IncidentDate,
            ExistingImagePaths = report.Images.Select(i => i.ImagePath).ToList()
        };

        await PopulateDropdowns(vm);
        ViewData["ActivePage"] = "Reports";
        ViewData["PageTitle"] = "Edit Report";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/DisasterReports\">Reports</a></li><li class=\"breadcrumb-item active\">Edit</li>";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
    public async Task<IActionResult> Edit(int id, DisasterReportViewModel vm)
    {
        ModelState.Remove("Images");
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(vm);
            return View(vm);
        }

        var report = await _context.DisasterReports.Include(r => r.Images).FirstOrDefaultAsync(r => r.Id == id);
        if (report == null) return NotFound();

        report.Title = vm.Title;
        report.Description = vm.Description;
        report.DisasterTypeId = vm.DisasterTypeId;
        report.Severity = vm.Severity;
        report.RegionId = vm.RegionId;
        report.DistrictId = vm.DistrictId;
        report.Location = vm.Location;
        report.Latitude = vm.Latitude;
        report.Longitude = vm.Longitude;
        report.IncidentDate = vm.IncidentDate.ToUniversalTime();
        report.UpdatedAt = DateTime.UtcNow;

        if (vm.Images != null)
        {
            foreach (var file in vm.Images.Where(f => f != null && f.Length > 0))
            {
                if (_fileUploadService.IsValidImage(file))
                {
                    var path = await _fileUploadService.UploadImageAsync(file, "reports");
                    if (path != null)
                    {
                        _context.ReportImages.Add(new ReportImage
                        {
                            DisasterReportId = report.Id,
                            ImagePath = path,
                            FileSizeBytes = file.Length,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(_userManager.GetUserId(User), "Disaster Report Updated", "DisasterReport", id.ToString());

        TempData["Success"] = "Report updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Verification pipeline: Review → Verify → Approve/Reject → Resolve ──

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> StartReview(int id)
        => await ChangeStatus(id, ReportStatus.UnderReview, null);

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Approve(int id, string? notes)
        => await ChangeStatus(id, ReportStatus.Verified, notes);

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Reject(int id, string? notes)
        => await ChangeStatus(id, ReportStatus.Rejected, notes);

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Resolve(int id)
        => await ChangeStatus(id, ReportStatus.Resolved, null);

    private async Task<IActionResult> ChangeStatus(int id, ReportStatus status, string? notes)
    {
        var report = await _context.DisasterReports.FindAsync(id);
        if (report == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        report.Status = status;
        report.UpdatedAt = DateTime.UtcNow;

        if (status == ReportStatus.Verified || status == ReportStatus.Rejected)
        {
            report.IsVerified = status == ReportStatus.Verified;
            report.VerifiedById = userId;
            report.VerifiedAt = DateTime.UtcNow;
            report.VerificationNotes = notes;
        }

        await _context.SaveChangesAsync();
        await _notificationService.CreateAsync(report.ReportedById,
            "Report Status Updated",
            $"Your disaster report \"{report.Title}\" status has been changed to {status}.",
            NotificationType.ReportUpdate, id, "DisasterReport");

        TempData["Success"] = $"Report status updated to {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await _context.DisasterReports.Include(r => r.Images).FirstOrDefaultAsync(r => r.Id == id);
        if (report == null) return NotFound();

        foreach (var img in report.Images)
            _fileUploadService.DeleteFile(img.ImagePath);

        _context.DisasterReports.Remove(report);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync(_userManager.GetUserId(User), "Disaster Report Deleted", "DisasterReport", id.ToString());

        TempData["Success"] = "Report deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetDistricts(int regionId)
    {
        var districts = await _context.Districts
            .Where(d => d.RegionId == regionId && d.IsActive)
            .Select(d => new { d.Id, d.Name })
            .ToListAsync();
        return Json(districts);
    }

    private async Task PopulateDropdowns(DisasterReportViewModel vm)
    {
        vm.DisasterTypes = new SelectList(
            await _context.DisasterTypes.Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync(),
            "Id", "Name", vm.DisasterTypeId);
        vm.Regions = new SelectList(
            await _context.Regions.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync(),
            "Id", "Name", vm.RegionId);
        vm.Districts = vm.RegionId.HasValue
            ? new SelectList(await _context.Districts.Where(d => d.RegionId == vm.RegionId && d.IsActive).ToListAsync(), "Id", "Name", vm.DistrictId)
            : new SelectList(Enumerable.Empty<object>(), "Id", "Name");
    }
}
