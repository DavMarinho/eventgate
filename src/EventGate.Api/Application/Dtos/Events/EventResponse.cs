namespace EventGate.Api.Application.Dtos.Events;

/// <summary>Representação pública de um evento.</summary>
public sealed record EventResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Location,
    DateTimeOffset StartsAt,
    int Capacity,
    int RegisteredCount);
