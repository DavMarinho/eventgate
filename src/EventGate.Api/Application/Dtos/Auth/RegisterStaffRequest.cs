using System.ComponentModel.DataAnnotations;
using EventGate.Api.Domain.Enums;

namespace EventGate.Api.Application.Dtos.Auth;

/// <summary>Criação de uma nova conta de equipe (somente Organizer pode chamar).</summary>
public sealed class RegisterStaffRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
}
