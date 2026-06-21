using System.ComponentModel.DataAnnotations;

namespace EventGate.Api.Application.Dtos.CheckIn;

/// <summary>Código apresentado na portaria para validação.</summary>
public sealed class ValidateCodeRequest
{
    [Required]
    public string AccessCode { get; set; } = string.Empty;
}
