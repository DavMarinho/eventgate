namespace EventGate.Api.Domain.Entities;

/// <summary>
/// Evento com lotação (Capacity) e um organizador responsável.
/// </summary>
public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    /// <summary>Lotação máxima. A inscrição é recusada quando atingida.</summary>
    public int Capacity { get; set; }

    public Guid OrganizerId { get; set; }

    public User? Organizer { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
