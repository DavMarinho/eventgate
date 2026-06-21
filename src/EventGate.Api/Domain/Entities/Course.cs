namespace EventGate.Api.Domain.Entities;

/// <summary>
/// Curso (lista controlada, seedada com cursos da USP). Mantém o dashboard
/// agrupando corretamente — texto livre só no caminho "Outro" da inscrição.
/// </summary>
public class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    /// <summary>Sigla/identificador opcional (ex.: unidade).</summary>
    public string? Code { get; set; }

    /// <summary>Permite ocultar um curso sem apagá-lo.</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
