namespace EventGate.Api.Domain.Entities;

/// <summary>
/// Registro de que uma inscrição entrou numa palestra específica.
/// Único por (SessionId, RegistrationId) — não dá pra marcar a mesma palestra 2x.
/// </summary>
public class SessionAttendance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }

    public Session? Session { get; set; }

    public Guid RegistrationId { get; set; }

    public Registration? Registration { get; set; }

    public DateTimeOffset CheckedInAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid CheckedInByUserId { get; set; }
}
