namespace EventGate.Api.Application.Dtos.CheckIn;

/// <summary>
/// Passo 1 da portaria: dados para a equipe conferir a pessoa (foto incluída como
/// data URI). Não consome o código. Código inválido devolve <see cref="Found"/> = false.
/// </summary>
public sealed record GateLookupResponse(
    bool Found,
    string Reason,
    Guid? RegistrationId = null,
    string? ParticipantName = null,
    string? Course = null,
    int? Semester = null,
    DateOnly? BirthDate = null,
    string? Email = null,
    string? Status = null,
    bool AlreadyCheckedIn = false,
    DateTimeOffset? CheckedInAt = null,
    string? PhotoDataUri = null);
