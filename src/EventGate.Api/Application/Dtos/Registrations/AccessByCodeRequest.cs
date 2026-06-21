using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Application.Dtos.Registrations;

/// <summary>
/// Autoatendimento LGPD do titular: prova de posse via código + e-mail.
/// Enviado no corpo (POST) — dados pessoais nunca trafegam na URL.
/// </summary>
public sealed class AccessByCodeRequest
{
    [Required]
    public string AccessCode { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
