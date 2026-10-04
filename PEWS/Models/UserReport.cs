using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PEWS.Models.Enums;

namespace PEWS.Models;

/// <summary>
/// An incident report submitted by a citizen through the mobile app. Kept separate from
/// <see cref="DisasterReport"/>, which is the admin-authored official record — citizen
/// submissions and staff records are independent stores by design (see UserReportsController
/// for the admin review pipeline: Pending -> UnderReview -> Approve/Reject -> Resolved).
/// </summary>
public class UserReport
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public int DisasterTypeId { get; set; }

    public SeverityLevel Severity { get; set; } = SeverityLevel.Medium;

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    [Required]
    public string ReportedById { get; set; } = string.Empty;

    public int? RegionId { get; set; }
    public int? DistrictId { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime IncidentDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsVerified { get; set; } = false;

    public string? VerifiedById { get; set; }

    public DateTime? VerifiedAt { get; set; }

    [MaxLength(1000)]
    public string? VerificationNotes { get; set; }

    [ForeignKey(nameof(DisasterTypeId))]
    public DisasterType DisasterType { get; set; } = null!;

    [ForeignKey(nameof(ReportedById))]
    public ApplicationUser ReportedBy { get; set; } = null!;

    [ForeignKey(nameof(VerifiedById))]
    public ApplicationUser? VerifiedBy { get; set; }

    [ForeignKey(nameof(RegionId))]
    public Region? Region { get; set; }

    [ForeignKey(nameof(DistrictId))]
    public District? District { get; set; }

    public ICollection<UserReportImage> Images { get; set; } = new List<UserReportImage>();
}
