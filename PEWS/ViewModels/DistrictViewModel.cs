using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PEWS.Models.Enums;

namespace PEWS.ViewModels;

public class DistrictViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Region")]
    public int RegionId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? Population { get; set; }

    [Display(Name = "Risk Level")]
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public SelectList? Regions { get; set; }
}
