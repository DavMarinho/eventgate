using EventGate.Api.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventGate.Api.Infrastructure.Notifications;

/// <summary>
/// Worker em segundo plano que consome a fila e envia os e-mails. Falha de envio
/// é logada e não derruba o processo — a inscrição já foi concluída.
/// </summary>
public sealed class EmailDispatcher(
    IEmailQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                await sender.SendAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao enviar e-mail para {To}.", message.ToEmail);
            }
        }
    }
}
