namespace EventGate.Api.Application.Interfaces;

/// <summary>Mensagem de e-mail com anexo opcional (ex.: QR em PNG).</summary>
public sealed record EmailMessage(
    string ToEmail,
    string ToName,
    string Subject,
    string HtmlContent,
    byte[]? AttachmentBytes = null,
    string? AttachmentName = null);

/// <summary>Envia um e-mail (implementação concreta: Brevo via HTTP API).</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

/// <summary>
/// Fila assíncrona de e-mails. O envio acontece em segundo plano para não
/// bloquear a inscrição nem fazê-la falhar caso o provedor esteja indisponível.
/// </summary>
public interface IEmailQueue
{
    void Enqueue(EmailMessage message);
    IAsyncEnumerable<EmailMessage> DequeueAllAsync(CancellationToken ct);
}
