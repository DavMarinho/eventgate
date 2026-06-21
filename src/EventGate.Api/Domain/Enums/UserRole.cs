namespace EventGate.Api.Domain.Enums;

/// <summary>
/// Perfis de acesso da equipe. Participantes não têm perfil (não são usuários).
/// </summary>
public enum UserRole
{
    /// <summary>Cria eventos, gerencia equipe e também pode validar na portaria.</summary>
    Organizer = 1,

    /// <summary>Menor privilégio: apenas valida códigos na portaria.</summary>
    Validator = 2
}
