using EventGate.Api.Application.Dtos.Dashboard;
using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Interfaces;

public interface ISessionRepository
{
    Task<IReadOnlyList<Session>> ListByEventAsync(Guid eventId, CancellationToken ct = default);
    Task<Session?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Session session, CancellationToken ct = default);
    Task RemoveAsync(Session session, CancellationToken ct = default);

    /// <summary>Presença por palestra do evento (para o dashboard).</summary>
    Task<IReadOnlyList<SessionStat>> GetStatsAsync(Guid eventId, CancellationToken ct = default);
}
