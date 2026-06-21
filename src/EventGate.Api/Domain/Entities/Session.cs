namespace EventGate.Api.Domain.Entities;

/// <summary>
/// Palestra/sessão de um evento. Cada uma tem sua própria porta onde a equipe
/// marca a presença (além da entrada principal do evento).
/// </summary>
public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventId { get; set; }

    public Event? Event { get; set; }

    public required string Title { get; set; }

    public string? Speaker { get; set; }

    public string? Room { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }
}
