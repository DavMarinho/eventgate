using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Application.Dtos.Courses;

/// <summary>Criação de um curso (somente Organizer).</summary>
public sealed class CreateCourseRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Code { get; set; }
}
