using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Event @event, CancellationToken ct = default);

    /// <summary>Inscrições ativas (não canceladas) — usado para checar lotação.</summary>
    Task<int> CountActiveRegistrationsAsync(Guid eventId, CancellationToken ct = default);
}
