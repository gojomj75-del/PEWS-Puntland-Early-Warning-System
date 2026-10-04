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
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PEWS.Controllers.Api;

// Serves the mobile app's citizen-submitted incident reports. These are stored in the
// UserReports table — separate from DisasterReports, which is the admin-authored official
// record on the web portal. See Models/UserReport.cs for why they're kept independent.
[ApiController]
[Route("api/reports")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAuditService _auditService;

    public ApiReportsController(
        ApplicationDbContext context,
        IFileUploadService fileUploadService,
        IAuditService auditService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReports(int page = 1, int pageSize = 20)
    {
        var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("EmergencyOfficer");

        var query = _context.UserReports
            .Include(r => r.DisasterType)
            .Include(r => r.Region)
            .Include(r => r.Images)
            .AsQueryable();
        if (!isAdmin) query = query.Where(r => r.ReportedById == userId);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReportListItemDto
            {
                Id           = r.Id,
                Title        = r.Title,
                DisasterType = r.DisasterType.Name,
                Severity     = r.Severity.ToString(),
                Status       = r.Status.ToString(),
                Region       = r.Region != null ? r.Region.Name : null,
                IsVerified   = r.IsVerified,
                ImageCount   = r.Images.Count,
                CreatedAt    = r.CreatedAt
            })
            .ToListAsync();

        return Ok(reports);
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        var types = await _context.DisasterTypes
            .Where(t => t.IsActive)
            .Select(t => new DisasterTypeDto { Id = t.Id, Name = t.Name })
            .ToListAsync();
        return Ok(types);
    }

    [HttpGet("regions")]
    public async Task<IActionResult> GetRegions()
    {
        var regions = await _context.Regions
            .Where(r => r.IsActive)
            .Select(r => new RegionDto { Id = r.Id, Name = r.Name })
            .ToListAsync();
        return Ok(regions);
    }

    [HttpPost]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> SubmitReport([FromForm] MobileReportRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var report = new UserReport
        {
            Title          = request.Title,
            Description    = request.Description,
            DisasterTypeId = request.DisasterTypeId,
            Severity       = (SeverityLevel)request.Severity,
            Location       = request.Location,
            RegionId       = request.RegionId,
            ReportedById   = userId,
            Status         = ReportStatus.Pending,
            IncidentDate   = DateTime.UtcNow,
            CreatedAt      = DateTime.UtcNow
        };

        _context.UserReports.Add(report);
        await _context.SaveChangesAsync();

        if (request.Images != null)
        {
            foreach (var file in request.Images.Where(f => f.Length > 0).Take(5))
            {
                if (_fileUploadService.IsValidImage(file))
                {
                    var path = await _fileUploadService.UploadImageAsync(file, "reports");
                    if (path == null) continue;
                    _context.UserReportImages.Add(new UserReportImage
                    {
                        UserReportId  = report.Id,
                        ImagePath     = path,
                        FileSizeBytes = file.Length,
                        UploadedAt    = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        await _auditService.LogAsync(userId, "Citizen Report Submitted", "UserReport", report.Id.ToString());

        return Ok(new ReportSubmitResponse
        {
            Id        = report.Id,
            Title     = report.Title,
            Status    = report.Status.ToString(),
            CreatedAt = report.CreatedAt
        });
    }
}

public class MobileReportRequest
{
    [Required] public string Title          { get; set; } = string.Empty;
    [Required] public string Description    { get; set; } = string.Empty;
    [Required] public int    DisasterTypeId { get; set; }
    public int     Severity       { get; set; } = 2;
    public string? Location       { get; set; }
    public int?    RegionId       { get; set; }
    public List<IFormFile>? Images { get; set; }
}
