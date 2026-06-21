using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure.Data.Repositories;

public sealed class SpeakerRepository(AppDbContext db) : ISpeakerRepository
{
    public async Task<IReadOnlyList<Speaker>> ListByEventAsync(Guid eventId, CancellationToken ct = default) =>
        await db.Speakers.AsNoTracking()
            .Where(s => s.EventId == eventId)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
            .ToListAsync(ct);

    public Task<Speaker?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Speakers.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(Speaker speaker, CancellationToken ct = default)
    {
        db.Speakers.Add(speaker);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Speaker speaker, CancellationToken ct = default)
    {
        db.Speakers.Remove(speaker);
        await db.SaveChangesAsync(ct);
    }
}
