using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PEWS.Models;
using PEWS.ViewModels;

namespace PEWS.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index(string? search, string? role, bool? isActive, int page = 1)
    {
        const int pageSize = 15;

        var usersQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            usersQuery = usersQuery.Where(u =>
                u.Email!.Contains(search) ||
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search));

        if (isActive.HasValue)
            usersQuery = usersQuery.Where(u => u.IsActive == isActive.Value);

        var allUsers = await usersQuery.OrderByDescending(u => u.CreatedAt).ToListAsync();

        // Filter by role (can't do this in EF directly with Identity)
        var userWithRoles = new List<(ApplicationUser User, IList<string> Roles)>();
        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (string.IsNullOrEmpty(role) || roles.Contains(role))
                userWithRoles.Add((user, roles));
        }

        var totalCount = userWithRoles.Count;
        var paged = userWithRoles.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Search = search;
        ViewBag.Role = role;
        ViewBag.IsActive = isActive;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;
        ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        ViewBag.UserWithRoles = paged;

        ViewData["ActivePage"] = "Users";
        ViewData["PageTitle"] = "User Management";
        return View(paged);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.Roles = await _userManager.GetRolesAsync(user);
        ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        ViewData["ActivePage"] = "Users";
        ViewData["PageTitle"] = "User Details";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/UserManagement\">Users</a></li><li class=\"breadcrumb-item active\">Details</li>";
        return View(user);
    }

    // ── Create staff account (Admin/EmergencyOfficer, or SuperAdmin if the actor is one) ──

    private SelectList AvailableRolesForCurrentUser()
    {
        var roles = new List<string> { "Admin", "EmergencyOfficer", "Citizen" };
        if (User.IsInRole("SuperAdmin"))
            roles.Insert(0, "SuperAdmin");
        return new SelectList(roles);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new CreateUserViewModel { AvailableRoles = AvailableRolesForCurrentUser() };
        ViewData["ActivePage"] = "Users";
        ViewData["PageTitle"] = "Add Staff Account";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/UserManagement\">Users</a></li><li class=\"breadcrumb-item active\">Add</li>";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel vm)
    {
        // Only a SuperAdmin may provision another SuperAdmin.
        if (vm.Role == "SuperAdmin" && !User.IsInRole("SuperAdmin"))
            ModelState.AddModelError(nameof(vm.Role), "Only a SuperAdmin can create another SuperAdmin account.");

        if (!ModelState.IsValid)
        {
            vm.AvailableRoles = AvailableRolesForCurrentUser();
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            PhoneNumber = vm.PhoneNumber,
            City = vm.City,
            Region = vm.Region,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            vm.AvailableRoles = AvailableRolesForCurrentUser();
            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, vm.Role);
        TempData["Success"] = $"{user.FullName} created as {vm.Role}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var vm = new UserEditViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            City = user.City,
            Region = user.Region
        };

        ViewData["ActivePage"] = "Users";
        ViewData["PageTitle"] = "Edit User";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/UserManagement\">Users</a></li><li class=\"breadcrumb-item active\">Edit</li>";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
            return View(vm);

        if (!string.Equals(user.Email, vm.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(vm.Email);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already in use.");
                return View(vm);
            }

            await _userManager.SetEmailAsync(user, vm.Email);
            await _userManager.SetUserNameAsync(user, vm.Email);
        }

        user.FirstName = vm.FirstName;
        user.LastName = vm.LastName;
        user.PhoneNumber = vm.PhoneNumber;
        user.Address = vm.Address;
        user.City = vm.City;
        user.Region = vm.Region;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(vm);
        }

        TempData["Success"] = "User details updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Prevent deactivating yourself
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToAction(nameof(Details), new { id });
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AssignRole(string userId, string roleName, string action)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        if (action == "add")
        {
            if (!await _userManager.IsInRoleAsync(user, roleName))
                await _userManager.AddToRoleAsync(user, roleName);
        }
        else
        {
            if (await _userManager.IsInRoleAsync(user, roleName))
                await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        TempData["Success"] = $"Role '{roleName}' {(action == "add" ? "assigned to" : "removed from")} {user.FullName}.";
        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(user);
        TempData["Success"] = "User deleted.";
        return RedirectToAction(nameof(Index));
    }
}
