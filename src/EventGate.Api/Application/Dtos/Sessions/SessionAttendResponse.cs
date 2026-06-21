namespace EventGate.Api.Application.Dtos.Sessions;

/// <summary>
/// Resultado de marcar presença numa palestra. Como na portaria principal,
/// não lança erro para casos esperados — devolve <see cref="Success"/> = false
/// com o motivo.
/// </summary>
public sealed record SessionAttendResponse(
    bool Success,
    string Reason,
    string? ParticipantName = null,
    string? SessionTitle = null,
    bool AlreadyAttended = false);
