namespace EventGate.Api.Application.Dtos.Registrations;

/// <summary>
/// Dados da inscrição (sem a foto — os bytes da foto são passados à parte pelo
/// controller, que recebe o <c>multipart/form-data</c>). Regras de negócio
/// (consentimento, curso exclusivo, semestre) são validadas no serviço.
/// </summary>
public sealed record CreateRegistrationRequest(
    string ParticipantName,
    string ParticipantEmail,
    DateOnly BirthDate,
    Guid? CourseId,
    string? CourseOther,
    int? Semester,
    bool ConsentAccepted);
