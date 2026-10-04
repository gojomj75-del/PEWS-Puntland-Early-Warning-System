using System.ComponentModel.DataAnnotations;
using PEWS.Models.Enums;

namespace PEWS.ViewModels;

public class RegionViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Risk Level")]
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
