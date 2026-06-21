using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure.Data.Repositories;

public sealed class SessionAttendanceRepository(AppDbContext db) : ISessionAttendanceRepository
{
    public Task<bool> ExistsAsync(Guid sessionId, Guid registrationId, CancellationToken ct = default) =>
        db.SessionAttendances.AnyAsync(a => a.SessionId == sessionId && a.RegistrationId == registrationId, ct);

    public async Task AddAsync(SessionAttendance attendance, CancellationToken ct = default)
    {
        db.SessionAttendances.Add(attendance);
        await db.SaveChangesAsync(ct);
    }
}
