using System.Threading.Channels;
using EventGate.Api.Application.Interfaces;

namespace EventGate.Api.Infrastructure.Notifications;

/// <summary>Fila em memória (Channel). Produtor: inscrição. Consumidor: <see cref="EmailDispatcher"/>.</summary>
public sealed class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>(
        new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(EmailMessage message) => _channel.Writer.TryWrite(message);

    public IAsyncEnumerable<EmailMessage> DequeueAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
