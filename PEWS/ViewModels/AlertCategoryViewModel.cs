using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PEWS.ViewModels;

public class AlertCategoryViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string Color { get; set; } = "#dc3545";

    [Required, MaxLength(100)]
    [Display(Name = "Icon Class")]
    public string IconClass { get; set; } = "fas fa-bell";

    [Display(Name = "Disaster Type")]
    public int? DisasterTypeId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public SelectList? DisasterTypes { get; set; }
}
