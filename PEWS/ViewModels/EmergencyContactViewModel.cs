using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PEWS.Models.Enums;

namespace PEWS.ViewModels;

public class EmergencyContactViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Contact Type")]
    public ContactType ContactType { get; set; }

    [Required, MaxLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Alternative Phone")]
    public string? AlternativePhone { get; set; }

    [EmailAddress, MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [Display(Name = "Region")]
    public int? RegionId { get; set; }

    [MaxLength(200)]
    [Display(Name = "Operating Hours")]
    public string? OperatingHours { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Display(Name = "24/7 Available")]
    public bool IsAvailable24Hours { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public SelectList? Regions { get; set; }
}
