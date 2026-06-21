using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Application.Dtos.Sessions;

public sealed class CreateSessionRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Speaker { get; set; }

    [StringLength(100)]
    public string? Room { get; set; }

    [Required]
    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }
}
