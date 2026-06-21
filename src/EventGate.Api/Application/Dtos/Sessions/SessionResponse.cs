namespace EventGate.Api.Application.Dtos.Sessions;

public sealed record SessionResponse(
    Guid Id,
    Guid EventId,
    string Title,
    string? Speaker,
    string? Room,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);
