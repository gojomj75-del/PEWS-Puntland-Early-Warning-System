using System.ComponentModel.DataAnnotations;

namespace PEWS.Models;

public class AlertCategory
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string Color { get; set; } = "#dc3545";

    [MaxLength(100)]
    public string IconClass { get; set; } = "fas fa-bell";

    public int? DisasterTypeId { get; set; }

    public bool IsActive { get; set; } = true;

    public DisasterType? DisasterType { get; set; }

    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
