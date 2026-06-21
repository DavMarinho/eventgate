namespace EventGate.Api.Application.Dtos.Registrations;

/// <summary>Item da lista de inscritos (painel da equipe). Sem foto.</summary>
public sealed record RegistrationListItem(
    Guid Id,
    string ParticipantName,
    string ParticipantEmail,
    string Course,
    int? Semester,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CheckedInAt);
