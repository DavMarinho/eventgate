using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Api.Models;

/// <summary>
/// Modelo do <c>multipart/form-data</c> da inscrição (inclui o arquivo da foto).
/// Fica na camada Api porque depende de <see cref="IFormFile"/> (tipo do framework web).
/// </summary>
public sealed class CreateRegistrationForm
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string ParticipantName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ParticipantEmail { get; set; } = string.Empty;

    [Required]
    public DateOnly BirthDate { get; set; }

    /// <summary>Curso da lista (USP). Mutuamente exclusivo com <see cref="CourseOther"/>.</summary>
    public Guid? CourseId { get; set; }

    /// <summary>Curso fora da lista ("Outro").</summary>
    [StringLength(200)]
    public string? CourseOther { get; set; }

    [Range(1, 12)]
    public int? Semester { get; set; }

    [Required]
    public bool ConsentAccepted { get; set; }

    [Required]
    public IFormFile Photo { get; set; } = default!;
}
