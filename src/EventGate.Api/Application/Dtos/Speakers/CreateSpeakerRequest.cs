using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Application.Dtos.Speakers;

public sealed class CreateSpeakerRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Role { get; set; }

    [StringLength(200)]
    public string? Talk { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    [StringLength(500)]
    [Url]
    public string? PhotoUrl { get; set; }

    public int SortOrder { get; set; }
}
