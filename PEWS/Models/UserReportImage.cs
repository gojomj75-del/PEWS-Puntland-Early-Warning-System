using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PEWS.Models;

public class UserReportImage
{
    public int Id { get; set; }

    public int UserReportId { get; set; }

    [Required]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Caption { get; set; }

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserReportId))]
    public UserReport UserReport { get; set; } = null!;
}
