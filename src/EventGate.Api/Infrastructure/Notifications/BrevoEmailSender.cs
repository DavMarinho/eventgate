using System.Net.Http.Json;
using EventGate.Api.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventGate.Api.Infrastructure.Notifications;

/// <summary>
/// Envia e-mail pela API transacional da Brevo (POST /v3/smtp/email).
/// Se a API key não estiver configurada, apenas registra um aviso e não envia
/// (útil em desenvolvimento, sem quebrar a inscrição).
/// </summary>
public sealed class BrevoEmailSender(
    HttpClient http,
    IOptions<BrevoSettings> options,
    ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private readonly BrevoSettings _settings = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            logger.LogWarning("Brevo:ApiKey não configurada — e-mail para {To} não enviado.", message.ToEmail);
            return;
        }

        var payload = new Dictionary<string, object?>
        {
            ["sender"] = new { email = _settings.SenderEmail, name = _settings.SenderName },
            ["to"] = new[] { new { email = message.ToEmail, name = message.ToName } },
            ["subject"] = message.Subject,
            ["htmlContent"] = message.HtmlContent
        };

        if (message.AttachmentBytes is { Length: > 0 })
        {
            payload["attachment"] = new[]
            {
                new
                {
                    content = Convert.ToBase64String(message.AttachmentBytes),
                    name = message.AttachmentName ?? "anexo.png"
                }
            };
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", _settings.ApiKey);
        request.Content = JsonContent.Create(payload);

        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Brevo retornou {Status}: {Body}", (int)response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }
    }
}
