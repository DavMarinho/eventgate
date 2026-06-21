using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Application.Dtos.Events;

/// <summary>Dados para criar um evento.</summary>
public sealed class CreateEventRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? Location { get; set; }

    [Required]
    public DateTimeOffset StartsAt { get; set; }

    [Range(1, 1_000_000)]
    public int Capacity { get; set; }
}
