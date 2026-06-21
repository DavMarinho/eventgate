namespace EventGate.Api.Domain.Entities;

/// <summary>
/// Trilha de auditoria. Registra cada validação na portaria (sucesso e recusa)
/// para rastreabilidade — exigência de boas práticas e da LGPD.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Ação executada, ex.: "CheckInValidated", "CheckInRejected".</summary>
    public required string Action { get; set; }

    public Guid? EventId { get; set; }

    public Guid? RegistrationId { get; set; }

    /// <summary>Usuário da equipe que executou a ação.</summary>
    public Guid? PerformedByUserId { get; set; }

    /// <summary>Detalhe livre. Não deve conter dados sensíveis desnecessários.</summary>
    public string? Detail { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
