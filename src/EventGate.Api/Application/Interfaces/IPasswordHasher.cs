namespace EventGate.Api.Application.Interfaces;

/// <summary>Abstração de hashing de senha. Implementação usa PBKDF2-HMAC-SHA256.</summary>
public interface IPasswordHasher
{
    /// <summary>Gera o hash (com salt aleatório embutido) de uma senha em texto puro.</summary>
    string Hash(string password);

    /// <summary>
    /// Verifica a senha contra o hash armazenado, em tempo constante.
    /// </summary>
    bool Verify(string password, string storedHash);
}
