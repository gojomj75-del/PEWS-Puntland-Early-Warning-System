using System.ComponentModel.DataAnnotations;

namespace PEWS.ViewModels.Api;

public class LoginRequest
{
    [Required] public string Email    { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required] public string FirstName   { get; set; } = string.Empty;
    [Required] public string LastName    { get; set; } = string.Empty;
    [Required] public string Email       { get; set; } = string.Empty;
    [Required] public string Password    { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City       { get; set; }
    public string? Region     { get; set; }
}

public class AuthResponse
{
    public string   Token     { get; set; } = string.Empty;
    public string   UserId    { get; set; } = string.Empty;
    public string   Email     { get; set; } = string.Empty;
    public string   FullName  { get; set; } = string.Empty;
    public string   Role      { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class DashboardStatsResponse
{
    public int TotalReports         { get; set; }
    public int ActiveAlerts         { get; set; }
    public int PendingReports       { get; set; }
    public int VerifiedReports      { get; set; }
    public int UnreadNotifications  { get; set; }
}

public class AlertDto
{
    public int      Id          { get; set; }
    public string   Title       { get; set; } = string.Empty;
    public string   Message     { get; set; } = string.Empty;
    public string   Severity    { get; set; } = string.Empty;
    public string?  Region      { get; set; }
    public string   Category    { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}

public class NotificationDto
{
    public int      Id        { get; set; }
    public string   Title     { get; set; } = string.Empty;
    public string   Message   { get; set; } = string.Empty;
    public string   Type      { get; set; } = string.Empty;
    public bool     IsRead    { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportSubmitResponse
{
    public int      Id        { get; set; }
    public string   Title     { get; set; } = string.Empty;
    public string   Status    { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ReportListItemDto
{
    public int      Id           { get; set; }
    public string   Title        { get; set; } = string.Empty;
    public string   DisasterType { get; set; } = string.Empty;
    public string   Severity     { get; set; } = string.Empty;
    public string   Status       { get; set; } = string.Empty;
    public string?  Region       { get; set; }
    public bool     IsVerified   { get; set; }
    public int      ImageCount   { get; set; }
    public DateTime CreatedAt    { get; set; }
}

public class DisasterTypeDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class RegionDto       { public int Id { get; set; } public string Name { get; set; } = string.Empty; }

public class ProfileResponse
{
    public string   Id          { get; set; } = string.Empty;
    public string   FirstName   { get; set; } = string.Empty;
    public string   LastName    { get; set; } = string.Empty;
    public string   Email       { get; set; } = string.Empty;
    public string?  PhoneNumber { get; set; }
    public string?  Address     { get; set; }
    public string?  City        { get; set; }
    public string?  Region      { get; set; }
    public DateTime CreatedAt   { get; set; }
}

public class UpdateProfileRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName  { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address     { get; set; }
    public string? City        { get; set; }
    public string? Region      { get; set; }
}

public class ChangePasswordRequest
{
    [Required] public string CurrentPassword { get; set; } = string.Empty;
    [Required] public string NewPassword     { get; set; } = string.Empty;
}
