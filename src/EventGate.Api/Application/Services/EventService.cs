using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Events;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Services;

public sealed class EventService(IEventRepository events)
{
    public async Task<EventResponse> CreateAsync(CreateEventRequest request, Guid organizerId, CancellationToken ct = default)
    {
        if (request.StartsAt <= DateTimeOffset.UtcNow)
        {
            throw new ValidationException("A data do evento deve ser no futuro.");
        }

        var @event = new Event
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Location = request.Location?.Trim(),
            StartsAt = request.StartsAt,
            Capacity = request.Capacity,
            OrganizerId = organizerId
        };

        await events.AddAsync(@event, ct);

        return ToResponse(@event, registeredCount: 0);
    }

    public async Task<IReadOnlyList<EventResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await events.GetAllAsync(ct);
        var result = new List<EventResponse>(list.Count);

        foreach (var ev in list)
        {
            var count = await events.CountActiveRegistrationsAsync(ev.Id, ct);
            result.Add(ToResponse(ev, count));
        }

        return result;
    }

    public async Task<EventResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var ev = await events.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Evento não encontrado.");

        var count = await events.CountActiveRegistrationsAsync(ev.Id, ct);
        return ToResponse(ev, count);
    }

    private static EventResponse ToResponse(Event ev, int registeredCount) => new(
        ev.Id,
        ev.Name,
        ev.Description,
        ev.Location,
        ev.StartsAt,
        ev.Capacity,
        registeredCount);
}
