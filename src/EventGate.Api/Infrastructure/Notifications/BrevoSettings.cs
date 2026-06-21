namespace EventGate.Api.Infrastructure.Notifications;

/// <summary>
/// Configuração da Brevo. A <see cref="ApiKey"/> NUNCA deve ir no appsettings
/// versionado — use User Secrets / variável de ambiente (Brevo__ApiKey).
/// </summary>
public sealed class BrevoSettings
{
    public const string SectionName = "Brevo";

    public string ApiKey { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = "no-reply@eventgate.local";
    public string SenderName { get; set; } = "EventGate";
}
