using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PEWS.Models.Enums;

namespace PEWS.Models;

public class District
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int RegionId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? Population { get; set; }

    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RegionId))]
    public Region Region { get; set; } = null!;

    public ICollection<DisasterReport> DisasterReports { get; set; } = new List<DisasterReport>();
    public ICollection<UserReport> UserReports { get; set; } = new List<UserReport>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
