using EventGate.Api.Domain.Enums;

namespace EventGate.Api.Domain.Entities;

/// <summary>
/// Inscrição de um participante num evento. Minimização de dados (LGPD):
/// nome, e-mail, foto (para conferência na portaria), nascimento e curso.
/// Sem CPF. A foto fica no banco e nunca é exposta publicamente.
/// </summary>
public class Registration
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventId { get; set; }

    public Event? Event { get; set; }

    public required string ParticipantName { get; set; }

    public required string ParticipantEmail { get; set; }

    /// <summary>Foto do participante (bytes). Servida só para a equipe.</summary>
    public required byte[] PhotoData { get; set; }

    /// <summary>Tipo MIME da foto, ex.: "image/jpeg".</summary>
    public required string PhotoContentType { get; set; }

    public DateOnly BirthDate { get; set; }

    /// <summary>Curso da lista (USP). Mutuamente exclusivo com <see cref="CourseOther"/>.</summary>
    public Guid? CourseId { get; set; }

    public Course? Course { get; set; }

    /// <summary>Curso fora da lista ("Outro"). Mutuamente exclusivo com <see cref="CourseId"/>.</summary>
    public string? CourseOther { get; set; }

    /// <summary>Semestre 1–12. Só preenchido quando há curso da lista.</summary>
    public int? Semester { get; set; }

    /// <summary>
    /// Código alfanumérico único, gerado com RNG criptográfico.
    /// Unicidade garantida em duas camadas (retry + índice único no banco).
    /// </summary>
    public required string AccessCode { get; set; }

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Registered;

    public bool ConsentAccepted { get; set; }

    public DateTimeOffset? ConsentAcceptedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CheckedInAt { get; set; }

    /// <summary>Nome do curso para exibição (lista ou "Outro").</summary>
    public string CourseDisplay => Course?.Name ?? CourseOther ?? "—";
}
