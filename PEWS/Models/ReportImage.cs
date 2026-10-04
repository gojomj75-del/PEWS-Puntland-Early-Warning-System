using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PEWS.Models;

public class ReportImage
{
    public int Id { get; set; }

    public int DisasterReportId { get; set; }

    [Required]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Caption { get; set; }

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DisasterReportId))]
    public DisasterReport DisasterReport { get; set; } = null!;
}
