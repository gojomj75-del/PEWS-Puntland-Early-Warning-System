using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PEWS.Models.Enums;

namespace PEWS.ViewModels;

public class DisasterReportViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MinLength(20, ErrorMessage = "Please provide at least 20 characters of description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Disaster type is required")]
    [Display(Name = "Disaster Type")]
    public int DisasterTypeId { get; set; }

    [Display(Name = "Severity Level")]
    public SeverityLevel Severity { get; set; } = SeverityLevel.Medium;

    [Display(Name = "Region")]
    public int? RegionId { get; set; }

    [Display(Name = "District")]
    public int? DistrictId { get; set; }

    [MaxLength(300)]
    [Display(Name = "Location / Address")]
    public string? Location { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [Required(ErrorMessage = "Incident date is required")]
    [Display(Name = "Incident Date & Time")]
    public DateTime IncidentDate { get; set; } = DateTime.Now;

    public List<IFormFile>? Images { get; set; }

    // For edit — existing images
    public List<string> ExistingImagePaths { get; set; } = new();

    // Dropdowns
    public SelectList? DisasterTypes { get; set; }
    public SelectList? Regions { get; set; }
    public SelectList? Districts { get; set; }
    public SelectList? Severities { get; set; }
}
