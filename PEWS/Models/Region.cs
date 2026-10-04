using System.ComponentModel.DataAnnotations;
using PEWS.Models.Enums;

namespace PEWS.Models;

public class Region
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<District> Districts { get; set; } = new List<District>();
    public ICollection<DisasterReport> DisasterReports { get; set; } = new List<DisasterReport>();
    public ICollection<UserReport> UserReports { get; set; } = new List<UserReport>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();
    public ICollection<WeatherData> WeatherData { get; set; } = new List<WeatherData>();
}
