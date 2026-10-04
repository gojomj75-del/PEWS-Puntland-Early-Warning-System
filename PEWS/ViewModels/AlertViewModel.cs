using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PEWS.Models.Enums;

namespace PEWS.ViewModels;

public class AlertViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [MinLength(10)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Alert Category")]
    public int AlertCategoryId { get; set; }

    [Display(Name = "Severity")]
    public SeverityLevel Severity { get; set; } = SeverityLevel.Medium;

    [Display(Name = "Region")]
    public int? RegionId { get; set; }

    [Display(Name = "District")]
    public int? DistrictId { get; set; }

    [Display(Name = "Expires At")]
    public DateTime? ExpiresAt { get; set; }

    public bool IsPublished { get; set; }

    public SelectList? AlertCategories { get; set; }
    public SelectList? Regions { get; set; }
    public SelectList? Districts { get; set; }
}
