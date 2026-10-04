using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PEWS.Data;
using PEWS.Models;
using PEWS.Models.Enums;
using PEWS.ViewModels;

namespace PEWS.Controllers;

[Authorize]
public class EmergencyContactsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmergencyContactsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(ContactType? type, int? regionId, string? search)
    {
        var query = _context.EmergencyContacts
            .Include(c => c.Region)
            .Where(c => c.IsActive)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(c => c.ContactType == type.Value);

        if (regionId.HasValue)
            query = query.Where(c => c.RegionId == regionId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || c.PhoneNumber.Contains(search));

        var contacts = await query.OrderBy(c => c.ContactType).ThenBy(c => c.Name).ToListAsync();

        ViewBag.Type = type;
        ViewBag.RegionId = regionId;
        ViewBag.Search = search;
        ViewBag.Regions = await _context.Regions.Where(r => r.IsActive).ToListAsync();

        ViewData["ActivePage"] = "Contacts";
        ViewData["PageTitle"] = "Emergency Contacts";
        return View(contacts);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create()
    {
        var vm = new EmergencyContactViewModel { IsActive = true };
        vm.Regions = new SelectList(await _context.Regions.Where(r => r.IsActive).ToListAsync(), "Id", "Name");
        ViewData["ActivePage"] = "Contacts";
        ViewData["PageTitle"] = "Add Emergency Contact";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create(EmergencyContactViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Regions = new SelectList(await _context.Regions.Where(r => r.IsActive).ToListAsync(), "Id", "Name");
            return View(vm);
        }

        _context.EmergencyContacts.Add(new EmergencyContact
        {
            Name = vm.Name,
            ContactType = vm.ContactType,
            PhoneNumber = vm.PhoneNumber,
            AlternativePhone = vm.AlternativePhone,
            Email = vm.Email,
            Address = vm.Address,
            RegionId = vm.RegionId,
            OperatingHours = vm.OperatingHours,
            Notes = vm.Notes,
            IsAvailable24Hours = vm.IsAvailable24Hours,
            IsActive = vm.IsActive,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "Emergency contact added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var contact = await _context.EmergencyContacts.FindAsync(id);
        if (contact == null) return NotFound();

        var vm = new EmergencyContactViewModel
        {
            Id = contact.Id,
            Name = contact.Name,
            ContactType = contact.ContactType,
            PhoneNumber = contact.PhoneNumber,
            AlternativePhone = contact.AlternativePhone,
            Email = contact.Email,
            Address = contact.Address,
            RegionId = contact.RegionId,
            OperatingHours = contact.OperatingHours,
            Notes = contact.Notes,
            IsAvailable24Hours = contact.IsAvailable24Hours,
            IsActive = contact.IsActive
        };
        vm.Regions = new SelectList(await _context.Regions.Where(r => r.IsActive).ToListAsync(), "Id", "Name", vm.RegionId);
        ViewData["ActivePage"] = "Contacts";
        ViewData["PageTitle"] = "Edit Emergency Contact";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Edit(int id, EmergencyContactViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Regions = new SelectList(await _context.Regions.Where(r => r.IsActive).ToListAsync(), "Id", "Name", vm.RegionId);
            return View(vm);
        }

        var contact = await _context.EmergencyContacts.FindAsync(id);
        if (contact == null) return NotFound();

        contact.Name = vm.Name;
        contact.ContactType = vm.ContactType;
        contact.PhoneNumber = vm.PhoneNumber;
        contact.AlternativePhone = vm.AlternativePhone;
        contact.Email = vm.Email;
        contact.Address = vm.Address;
        contact.RegionId = vm.RegionId;
        contact.OperatingHours = vm.OperatingHours;
        contact.Notes = vm.Notes;
        contact.IsAvailable24Hours = vm.IsAvailable24Hours;
        contact.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Contact updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _context.EmergencyContacts.FindAsync(id);
        if (contact == null) return NotFound();
        _context.EmergencyContacts.Remove(contact);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Contact deleted.";
        return RedirectToAction(nameof(Index));
    }
}
