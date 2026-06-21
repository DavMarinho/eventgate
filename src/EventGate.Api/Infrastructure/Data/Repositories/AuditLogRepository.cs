using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Infrastructure.Data.Repositories;

public sealed class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        db.AuditLogs.Add(log);
        await db.SaveChangesAsync(ct);
    }
}
