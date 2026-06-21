using EventGate.Api.Application.Dtos.Dashboard;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure.Data.Repositories;

public sealed class SessionRepository(AppDbContext db) : ISessionRepository
{
    public async Task<IReadOnlyList<Session>> ListByEventAsync(Guid eventId, CancellationToken ct = default)
    {
        // Ordena em memória: SQLite não traduz ORDER BY de DateTimeOffset.
        var sessions = await db.Sessions.AsNoTracking().Where(s => s.EventId == eventId).ToListAsync(ct);
        return sessions.OrderBy(s => s.StartsAt).ToList();
    }

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Sessions.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Session session, CancellationToken ct = default)
    {
        db.Sessions.Add(session);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Session session, CancellationToken ct = default)
    {
        db.Sessions.Remove(session);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SessionStat>> GetStatsAsync(Guid eventId, CancellationToken ct = default)
    {
        var rows = await db.Sessions.AsNoTracking()
            .Where(s => s.EventId == eventId)
            .Select(s => new
            {
                s.Title,
                s.StartsAt,
                Attendance = db.SessionAttendances.Count(a => a.SessionId == s.Id)
            })
            .ToListAsync(ct);

        return rows
            .OrderBy(r => r.StartsAt)
            .Select(r => new SessionStat(r.Title, r.Attendance))
            .ToList();
    }
}
