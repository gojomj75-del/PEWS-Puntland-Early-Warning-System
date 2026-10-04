using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PEWS.Models;
using PEWS.Services;
using PEWS.ViewModels;

namespace PEWS.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(INotificationService notificationService, UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var notifications = await _notificationService.GetRecentAsync(userId, 50);
        ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(userId);
        ViewData["ActivePage"] = "Notifications";
        ViewData["PageTitle"] = "Notifications";
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        await _notificationService.MarkReadAsync(id, userId);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _userManager.GetUserId(User)!;
        await _notificationService.MarkAllReadAsync(userId);
        TempData["Success"] = "All notifications marked as read.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult Compose()
    {
        ViewData["ActivePage"] = "Notifications";
        ViewData["PageTitle"] = "Compose Notification";
        ViewData["Breadcrumb"] = "<li class=\"breadcrumb-item\"><a href=\"/Notifications\">Notifications</a></li><li class=\"breadcrumb-item active\">Compose</li>";
        return View(new ComposeNotificationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Compose(ComposeNotificationViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        await _notificationService.BroadcastAsync(vm.Title, vm.Message, vm.Type);

        TempData["Success"] = "Notification sent to all active users.";
        return RedirectToAction(nameof(Index));
    }
}
