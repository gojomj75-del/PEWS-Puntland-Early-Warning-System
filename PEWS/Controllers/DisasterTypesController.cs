using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models;
using PEWS.ViewModels;

namespace PEWS.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class DisasterTypesController : Controller
{
    private readonly ApplicationDbContext _context;

    public DisasterTypesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var types = await _context.DisasterTypes
            .Include(t => t.DisasterReports)
            .Include(t => t.AlertCategories)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .ToListAsync();

        ViewData["ActivePage"] = "DisasterTypes";
        ViewData["PageTitle"] = "Disaster Types";
        return View(types);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["ActivePage"] = "DisasterTypes";
        ViewData["PageTitle"] = "Add Disaster Type";
        return View(new DisasterTypeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DisasterTypeViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        _context.DisasterTypes.Add(new DisasterType
        {
            Name = vm.Name,
            Description = vm.Description,
            IconClass = vm.IconClass,
            Color = vm.Color,
            SortOrder = vm.SortOrder,
            IsActive = vm.IsActive
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "Disaster type added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var type = await _context.DisasterTypes.FindAsync(id);
        if (type == null) return NotFound();

        var vm = new DisasterTypeViewModel
        {
            Id = type.Id,
            Name = type.Name,
            Description = type.Description,
            IconClass = type.IconClass,
            Color = type.Color,
            SortOrder = type.SortOrder,
            IsActive = type.IsActive
        };

        ViewData["ActivePage"] = "DisasterTypes";
        ViewData["PageTitle"] = "Edit Disaster Type";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DisasterTypeViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (!ModelState.IsValid)
            return View(vm);

        var type = await _context.DisasterTypes.FindAsync(id);
        if (type == null) return NotFound();

        type.Name = vm.Name;
        type.Description = vm.Description;
        type.IconClass = vm.IconClass;
        type.Color = vm.Color;
        type.SortOrder = vm.SortOrder;
        type.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Disaster type updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await _context.DisasterTypes
            .Include(t => t.DisasterReports)
            .Include(t => t.AlertCategories)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (type == null) return NotFound();

        if (type.DisasterReports.Any() || type.AlertCategories.Any())
        {
            TempData["Error"] = $"Cannot delete \"{type.Name}\" — it is still used by existing reports or alert categories. Deactivate it instead.";
            return RedirectToAction(nameof(Index));
        }

        _context.DisasterTypes.Remove(type);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"\"{type.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
