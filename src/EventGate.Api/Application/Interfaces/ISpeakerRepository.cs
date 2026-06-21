using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Interfaces;

public interface ISpeakerRepository
{
    Task<IReadOnlyList<Speaker>> ListByEventAsync(Guid eventId, CancellationToken ct = default);
    Task<Speaker?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Speaker speaker, CancellationToken ct = default);
    Task RemoveAsync(Speaker speaker, CancellationToken ct = default);
}
