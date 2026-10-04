using System.ComponentModel.DataAnnotations;

namespace PEWS.Models;

public class DisasterType
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string IconClass { get; set; } = "fas fa-exclamation-triangle";

    [MaxLength(20)]
    public string Color { get; set; } = "#dc3545";

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    public ICollection<DisasterReport> DisasterReports { get; set; } = new List<DisasterReport>();
    public ICollection<UserReport> UserReports { get; set; } = new List<UserReport>();
    public ICollection<AlertCategory> AlertCategories { get; set; } = new List<AlertCategory>();
}
