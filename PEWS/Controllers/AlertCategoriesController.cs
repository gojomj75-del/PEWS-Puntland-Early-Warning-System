using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models;
using PEWS.ViewModels;

namespace PEWS.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class AlertCategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AlertCategoriesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var categories = await _context.AlertCategories
            .Include(c => c.DisasterType)
            .Include(c => c.Alerts)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewData["ActivePage"] = "AlertCategories";
        ViewData["PageTitle"] = "Alert Categories";
        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new AlertCategoryViewModel();
        vm.DisasterTypes = new SelectList(await _context.DisasterTypes.Where(t => t.IsActive).ToListAsync(), "Id", "Name");
        ViewData["ActivePage"] = "AlertCategories";
        ViewData["PageTitle"] = "Add Alert Category";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AlertCategoryViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.DisasterTypes = new SelectList(await _context.DisasterTypes.Where(t => t.IsActive).ToListAsync(), "Id", "Name", vm.DisasterTypeId);
            return View(vm);
        }

        _context.AlertCategories.Add(new AlertCategory
        {
            Name = vm.Name,
            Description = vm.Description,
            Color = vm.Color,
            IconClass = vm.IconClass,
            DisasterTypeId = vm.DisasterTypeId,
            IsActive = vm.IsActive
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "Alert category added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.AlertCategories.FindAsync(id);
        if (category == null) return NotFound();

        var vm = new AlertCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            IconClass = category.IconClass,
            DisasterTypeId = category.DisasterTypeId,
            IsActive = category.IsActive
        };
        vm.DisasterTypes = new SelectList(await _context.DisasterTypes.Where(t => t.IsActive).ToListAsync(), "Id", "Name", vm.DisasterTypeId);

        ViewData["ActivePage"] = "AlertCategories";
        ViewData["PageTitle"] = "Edit Alert Category";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AlertCategoryViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.DisasterTypes = new SelectList(await _context.DisasterTypes.Where(t => t.IsActive).ToListAsync(), "Id", "Name", vm.DisasterTypeId);
            return View(vm);
        }

        var category = await _context.AlertCategories.FindAsync(id);
        if (category == null) return NotFound();

        category.Name = vm.Name;
        category.Description = vm.Description;
        category.Color = vm.Color;
        category.IconClass = vm.IconClass;
        category.DisasterTypeId = vm.DisasterTypeId;
        category.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Alert category updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.AlertCategories
            .Include(c => c.Alerts)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();

        if (category.Alerts.Any())
        {
            TempData["Error"] = $"Cannot delete \"{category.Name}\" — it is still used by existing alerts. Deactivate it instead.";
            return RedirectToAction(nameof(Index));
        }

        _context.AlertCategories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"\"{category.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
