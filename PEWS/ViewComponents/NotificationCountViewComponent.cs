using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PEWS.Models;
using PEWS.Services;

namespace PEWS.ViewComponents;

public class NotificationCountViewComponent : ViewComponent
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationCountViewComponent(
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (UserClaimsPrincipal.Identity?.IsAuthenticated != true)
            return View(0);

        var userId = _userManager.GetUserId(UserClaimsPrincipal);
        if (string.IsNullOrEmpty(userId))
            return View(0);

        var count = await _notificationService.GetUnreadCountAsync(userId);
        return View(count);
    }
}
