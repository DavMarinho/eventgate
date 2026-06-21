namespace EventGate.Api.Application.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(
        string action,
        Guid? eventId = null,
        Guid? registrationId = null,
        Guid? performedByUserId = null,
        string? detail = null,
        CancellationToken ct = default);
}
