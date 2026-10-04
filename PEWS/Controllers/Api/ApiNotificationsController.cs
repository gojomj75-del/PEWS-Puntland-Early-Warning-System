using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PEWS.Services;
using PEWS.ViewModels.Api;
using System.Security.Claims;

namespace PEWS.Controllers.Api;

[ApiController]
[Route("api/notifications")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiNotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public ApiNotificationsController(INotificationService notificationService)
        => _notificationService = notificationService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notifications = await _notificationService.GetRecentAsync(userId, 50);

        return Ok(notifications.Select(n => new NotificationDto
        {
            Id        = n.Id,
            Title     = n.Title,
            Message   = n.Message,
            Type      = n.Type.ToString(),
            IsRead    = n.IsRead,
            CreatedAt = n.CreatedAt
        }));
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationService.MarkReadAsync(id, userId);
        return Ok();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationService.MarkAllReadAsync(userId);
        return Ok();
    }
}
