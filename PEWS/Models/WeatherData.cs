using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PEWS.Models;

public class WeatherData
{
    public int Id { get; set; }

    public int RegionId { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    [Range(-100, 100)]
    public double? Temperature { get; set; }

    [Range(0, 100)]
    public double? Humidity { get; set; }

    public double? WindSpeed { get; set; }

    public double? Precipitation { get; set; }

    [MaxLength(100)]
    public string? Condition { get; set; }

    [MaxLength(50)]
    public string? WindDirection { get; set; }

    [MaxLength(100)]
    public string? Source { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RegionId))]
    public Region Region { get; set; } = null!;
}
