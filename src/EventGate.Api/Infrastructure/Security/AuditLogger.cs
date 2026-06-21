using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Infrastructure.Security;

public sealed class AuditLogger(IAuditLogRepository repository) : IAuditLogger
{
    public Task LogAsync(
        string action,
        Guid? eventId = null,
        Guid? registrationId = null,
        Guid? performedByUserId = null,
        string? detail = null,
        CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Action = action,
            EventId = eventId,
            RegistrationId = registrationId,
            PerformedByUserId = performedByUserId,
            Detail = detail
        };

        return repository.AddAsync(log, ct);
    }
}
