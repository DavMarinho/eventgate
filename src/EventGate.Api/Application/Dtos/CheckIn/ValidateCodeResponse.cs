namespace EventGate.Api.Application.Dtos.CheckIn;

/// <summary>
/// Resultado da validação na portaria. Nunca lança erro para "código inválido"
/// ou "reuso": devolve <see cref="Valid"/> = false com o motivo, evitando dar
/// pistas a quem tenta enumerar códigos.
/// </summary>
public sealed record ValidateCodeResponse(
    bool Valid,
    string Reason,
    string? ParticipantName = null,
    Guid? EventId = null,
    DateTimeOffset? CheckedInAt = null);
