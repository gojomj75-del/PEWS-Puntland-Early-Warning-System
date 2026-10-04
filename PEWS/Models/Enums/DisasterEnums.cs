namespace PEWS.Models.Enums;

public enum SeverityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ReportStatus
{
    Pending = 1,
    UnderReview = 2,
    Verified = 3,
    Resolved = 4,
    Rejected = 5
}

public enum AlertStatus
{
    Draft = 1,
    Published = 2,
    Expired = 3
}

public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ContactType
{
    Hospital = 1,
    Police = 2,
    FireDepartment = 3,
    Ambulance = 4,
    DisasterResponse = 5,
    Government = 6,
    RedCross = 7
}

public enum NotificationType
{
    DisasterAlert = 1,
    ReportUpdate = 2,
    SystemNotification = 3,
    WeatherAlert = 4
}
