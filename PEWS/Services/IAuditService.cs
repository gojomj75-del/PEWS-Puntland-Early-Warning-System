namespace PEWS.Services;

public interface IAuditService
{
    Task LogAsync(string? userId, string action, string? entityType = null, string? entityId = null,
        string? oldValues = null, string? newValues = null, string? ipAddress = null);
}
