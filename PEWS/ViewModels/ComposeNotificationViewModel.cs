using System.ComponentModel.DataAnnotations;
using PEWS.Models.Enums;

namespace PEWS.ViewModels;

public class ComposeNotificationViewModel
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "Notification Type")]
    public NotificationType Type { get; set; } = NotificationType.SystemNotification;
}
