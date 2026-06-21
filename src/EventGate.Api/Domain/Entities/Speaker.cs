namespace EventGate.Api.Domain.Entities;

/// <summary>
/// Palestrante de um evento — usado na divulgação (landing page).
/// Conteúdo editável pela equipe (Organizer).
/// </summary>
public class Speaker
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventId { get; set; }

    public Event? Event { get; set; }

    public required string Name { get; set; }

    /// <summary>Cargo/instituição, ex.: "USP · Computação".</summary>
    public string? Role { get; set; }

    /// <summary>Título da palestra.</summary>
    public string? Talk { get; set; }

    public string? Bio { get; set; }

    /// <summary>URL da foto (opcional). Sem foto, a UI mostra as iniciais.</summary>
    public string? PhotoUrl { get; set; }

    /// <summary>Ordem de exibição (menor primeiro).</summary>
    public int SortOrder { get; set; }
}
