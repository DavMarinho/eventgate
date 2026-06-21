namespace EventGate.Api.Application.Dtos.Registrations;

/// <summary>
/// Dados da inscrição. Inclui o código de acesso — devolvido ao participante na
/// inscrição e no autoatendimento LGPD. Não inclui a foto.
/// </summary>
public sealed record RegistrationResponse(
    Guid Id,
    Guid EventId,
    string ParticipantName,
    string ParticipantEmail,
    string AccessCode,
    string Status,
    string Course,
    int? Semester,
    DateOnly BirthDate,
    bool ConsentAccepted,
    DateTimeOffset? ConsentAcceptedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CheckedInAt);
