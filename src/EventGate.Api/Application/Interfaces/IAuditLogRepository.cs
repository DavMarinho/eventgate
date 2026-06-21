using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
}
