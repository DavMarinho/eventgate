namespace EventGate.Api.Application.Dtos.CheckIn;

/// <summary>Estatísticas de presença de um evento.</summary>
public sealed record CheckInStatsResponse(
    Guid EventId,
    int Capacity,
    int TotalRegistered,
    int CheckedIn,
    int Pending);
