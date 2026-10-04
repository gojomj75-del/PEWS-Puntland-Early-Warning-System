using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PEWS.Models.Enums;

namespace PEWS.Models;

public class Alert
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public int AlertCategoryId { get; set; }

    public SeverityLevel Severity { get; set; } = SeverityLevel.Medium;

    public AlertStatus Status { get; set; } = AlertStatus.Draft;

    public int? RegionId { get; set; }
    public int? DistrictId { get; set; }

    [Required]
    public string CreatedById { get; set; } = string.Empty;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsPublished { get; set; } = false;

    public int ViewCount { get; set; } = 0;

    [ForeignKey(nameof(AlertCategoryId))]
    public AlertCategory AlertCategory { get; set; } = null!;

    [ForeignKey(nameof(RegionId))]
    public Region? Region { get; set; }

    [ForeignKey(nameof(DistrictId))]
    public District? District { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public ApplicationUser CreatedBy { get; set; } = null!;
}
