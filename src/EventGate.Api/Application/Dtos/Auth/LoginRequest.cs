using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Application.Dtos.Auth;

/// <summary>Credenciais de login da equipe.</summary>
public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
