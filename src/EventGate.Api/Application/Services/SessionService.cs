using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Sessions;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Services;

public sealed class SessionService(IEventRepository events, ISessionRepository sessions)
{
    public async Task<IReadOnlyList<SessionResponse>> ListByEventAsync(Guid eventId, CancellationToken ct = default)
    {
        var list = await sessions.ListByEventAsync(eventId, ct);
        return list.Select(ToResponse).ToList();
    }

    public async Task<SessionResponse> CreateAsync(Guid eventId, CreateSessionRequest request, CancellationToken ct = default)
    {
        _ = await events.GetByIdAsync(eventId, ct)
            ?? throw new NotFoundException("Evento não encontrado.");

        if (request.EndsAt is { } ends && ends <= request.StartsAt)
        {
            throw new ValidationException("O fim da palestra deve ser depois do início.");
        }

        var session = new Session
        {
            EventId = eventId,
            Title = request.Title.Trim(),
            Speaker = request.Speaker?.Trim(),
            Room = request.Room?.Trim(),
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt
        };

        await sessions.AddAsync(session, ct);
        return ToResponse(session);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var session = await sessions.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Palestra não encontrada.");

        await sessions.RemoveAsync(session, ct);
    }

    private static SessionResponse ToResponse(Session s) =>
        new(s.Id, s.EventId, s.Title, s.Speaker, s.Room, s.StartsAt, s.EndsAt);
}
