using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models;
using PEWS.Models.Enums;
using PEWS.ViewModels;

namespace PEWS.Controllers;

[Authorize(Roles = "SuperAdmin,Admin,EmergencyOfficer")]
public class RiskMonitoringController : Controller
{
    private readonly ApplicationDbContext _context;

    public RiskMonitoringController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var regions = await _context.Regions
            .Include(r => r.Districts)
            .Include(r => r.UserReports)
            .OrderByDescending(r => r.RiskLevel)
            .ToListAsync();

        ViewData["ActivePage"] = "Risk";
        ViewData["PageTitle"] = "Risk Monitoring";
        return View(regions);
    }

    public async Task<IActionResult> RegionDetails(int id)
    {
        var region = await _context.Regions
            .Include(r => r.Districts)
            .Include(r => r.UserReports.OrderByDescending(d => d.CreatedAt).Take(10))
                .ThenInclude(d => d.DisasterType)
            .Include(r => r.WeatherData.OrderByDescending(w => w.RecordedAt).Take(5))
            .FirstOrDefaultAsync(r => r.Id == id);

        if (region == null) return NotFound();

        ViewData["ActivePage"] = "Risk";
        ViewData["PageTitle"] = $"Risk Details — {region.Name}";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/RiskMonitoring\">Risk Monitoring</a></li><li class=\"breadcrumb-item active\">" + region.Name + "</li>";
        return View(region);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateRegionRisk(int id, RiskLevel riskLevel)
    {
        var region = await _context.Regions.FindAsync(id);
        if (region == null) return NotFound();

        region.RiskLevel = riskLevel;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"{region.Name} risk level updated to {riskLevel}.";
        return RedirectToAction(nameof(RegionDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateDistrictRisk(int id, RiskLevel riskLevel)
    {
        var district = await _context.Districts.FindAsync(id);
        if (district == null) return NotFound();

        district.RiskLevel = riskLevel;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"{district.Name} risk level updated to {riskLevel}.";
        return RedirectToAction(nameof(RegionDetails), new { id = district.RegionId });
    }

    // ── Region CRUD ──────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult CreateRegion()
    {
        ViewData["ActivePage"] = "Risk";
        ViewData["PageTitle"] = "Add Region";
        return View(new RegionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateRegion(RegionViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        _context.Regions.Add(new Region
        {
            Name = vm.Name,
            Code = vm.Code,
            Description = vm.Description,
            RiskLevel = vm.RiskLevel,
            Latitude = vm.Latitude,
            Longitude = vm.Longitude,
            IsActive = vm.IsActive,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "Region added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> EditRegion(int id)
    {
        var region = await _context.Regions.FindAsync(id);
        if (region == null) return NotFound();

        var vm = new RegionViewModel
        {
            Id = region.Id,
            Name = region.Name,
            Code = region.Code,
            Description = region.Description,
            RiskLevel = region.RiskLevel,
            Latitude = region.Latitude,
            Longitude = region.Longitude,
            IsActive = region.IsActive
        };

        ViewData["ActivePage"] = "Risk";
        ViewData["PageTitle"] = "Edit Region";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> EditRegion(int id, RegionViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (!ModelState.IsValid)
            return View(vm);

        var region = await _context.Regions.FindAsync(id);
        if (region == null) return NotFound();

        region.Name = vm.Name;
        region.Code = vm.Code;
        region.Description = vm.Description;
        region.RiskLevel = vm.RiskLevel;
        region.Latitude = vm.Latitude;
        region.Longitude = vm.Longitude;
        region.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Region updated.";
        return RedirectToAction(nameof(RegionDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteRegion(int id)
    {
        var region = await _context.Regions
            .Include(r => r.Districts)
            .Include(r => r.DisasterReports)
            .Include(r => r.UserReports)
            .Include(r => r.Alerts)
            .Include(r => r.EmergencyContacts)
            .Include(r => r.WeatherData)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (region == null) return NotFound();

        if (region.Districts.Any() || region.DisasterReports.Any() || region.UserReports.Any() || region.Alerts.Any()
            || region.EmergencyContacts.Any() || region.WeatherData.Any())
        {
            TempData["Error"] = $"Cannot delete {region.Name} — it still has districts or linked records. Remove those first, or deactivate the region instead.";
            return RedirectToAction(nameof(RegionDetails), new { id });
        }

        _context.Regions.Remove(region);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"{region.Name} deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── District CRUD ────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateDistrict(int? regionId)
    {
        var vm = new DistrictViewModel { RegionId = regionId ?? 0 };
        vm.Regions = new SelectList(await _context.Regions.Where(r => r.IsActive).ToListAsync(), "Id", "Name", regionId);
        ViewData["ActivePage"] = "Risk";
        ViewData["PageTitle"] = "Add District";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateDistrict(DistrictViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Regions = new SelectList(await _context.Regions.Where(r => r.IsActive).ToListAsync(), "Id", "Name", vm.RegionId);
            return View(vm);
        }

        _context.Districts.Add(new District
        {
            Name = vm.Name,
            RegionId = vm.RegionId,
            Description = vm.Description,
            Population = vm.Population,
            RiskLevel = vm.RiskLevel,
            Latitude = vm.Latitude,
            Longitude = vm.Longitude,
            IsActive = vm.IsActive,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "District added.";
        return RedirectToAction(nameof(RegionDetails), new { id = vm.RegionId });
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> EditDistrict(int id)
    {
        var district = await _context.Districts.FindAsync(id);
        if (district == null) return NotFound();

        var vm = new DistrictViewModel
        {
            Id = district.Id,
            Name = district.Name,
            RegionId = district.RegionId,
            Description = district.Description,
            Population = district.Population,
            RiskLevel = district.RiskLevel,
            Latitude = district.Latitude,
            Longitude = district.Longitude,
            IsActive = district.IsActive
        };
        vm.Regions = new SelectList(await _context.Regions.Where(r => r.IsActive).ToListAsync(), "Id", "Name", vm.RegionId);

        ViewData["ActivePage"] = "Risk";
        ViewData["PageTitle"] = "Edit District";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> EditDistrict(int id, DistrictViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.Regions = new SelectList(await _context.Regions.Where(r => r.IsActive).ToListAsync(), "Id", "Name", vm.RegionId);
            return View(vm);
        }

        var district = await _context.Districts.FindAsync(id);
        if (district == null) return NotFound();

        district.Name = vm.Name;
        district.RegionId = vm.RegionId;
        district.Description = vm.Description;
        district.Population = vm.Population;
        district.RiskLevel = vm.RiskLevel;
        district.Latitude = vm.Latitude;
        district.Longitude = vm.Longitude;
        district.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "District updated.";
        return RedirectToAction(nameof(RegionDetails), new { id = district.RegionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteDistrict(int id)
    {
        var district = await _context.Districts
            .Include(d => d.DisasterReports)
            .Include(d => d.UserReports)
            .Include(d => d.Alerts)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (district == null) return NotFound();

        if (district.DisasterReports.Any() || district.UserReports.Any() || district.Alerts.Any())
        {
            TempData["Error"] = $"Cannot delete {district.Name} — it still has linked reports or alerts. Deactivate it instead.";
            return RedirectToAction(nameof(RegionDetails), new { id = district.RegionId });
        }

        var regionId = district.RegionId;
        _context.Districts.Remove(district);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"{district.Name} deleted.";
        return RedirectToAction(nameof(RegionDetails), new { id = regionId });
    }
}
