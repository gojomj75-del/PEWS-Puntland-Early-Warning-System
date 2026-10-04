using System.ComponentModel.DataAnnotations;

namespace PEWS.ViewModels;

public class DisasterTypeViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Icon Class")]
    public string IconClass { get; set; } = "fas fa-exclamation-triangle";

    [Required, MaxLength(20)]
    public string Color { get; set; } = "#dc3545";

    [Display(Name = "Sort Order")]
    public int SortOrder { get; set; } = 0;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
