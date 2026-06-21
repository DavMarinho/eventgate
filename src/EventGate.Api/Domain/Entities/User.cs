using EventGate.Api.Domain.Enums;

namespace EventGate.Api.Domain.Entities;

/// <summary>
/// Conta de equipe (organizador ou validador). A senha nunca é armazenada em
/// texto puro — apenas o hash PBKDF2 com salt embutido (ver PasswordHasher).
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Login. Único no sistema.</summary>
    public required string Email { get; set; }

    /// <summary>Hash da senha no formato "iteracoes.saltBase64.hashBase64".</summary>
    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.Validator;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Eventos dos quais este usuário é organizador.</summary>
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
}
