namespace EventGate.Api.Application.Dtos.Speakers;

public sealed record SpeakerResponse(
    Guid Id,
    Guid EventId,
    string Name,
    string? Role,
    string? Talk,
    string? Bio,
    string? PhotoUrl,
    int SortOrder);
