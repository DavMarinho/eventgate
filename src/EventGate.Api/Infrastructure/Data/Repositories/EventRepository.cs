using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure.Data.Repositories;

public sealed class EventRepository(AppDbContext db) : IEventRepository
{
    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default)
    {
        // Ordena em memória: SQLite não traduz ORDER BY de DateTimeOffset.
        var events = await db.Events.AsNoTracking().ToListAsync(ct);
        return events.OrderBy(e => e.StartsAt).ToList();
    }

    public async Task AddAsync(Event @event, CancellationToken ct = default)
    {
        db.Events.Add(@event);
        await db.SaveChangesAsync(ct);
    }

    public Task<int> CountActiveRegistrationsAsync(Guid eventId, CancellationToken ct = default) =>
        db.Registrations.CountAsync(
            r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled, ct);
}
