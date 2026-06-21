using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Application.Dtos.Sessions;

/// <summary>Código apresentado na porta de uma palestra.</summary>
public sealed class SessionAttendRequest
{
    [Required]
    public string AccessCode { get; set; } = string.Empty;
}
