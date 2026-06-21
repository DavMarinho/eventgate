namespace EventGate.Api.Application.Interfaces;

/// <summary>
/// Gera códigos de acesso únicos com RNG criptograficamente seguro,
/// usando alfabeto sem caracteres ambíguos.
/// </summary>
public interface IAccessCodeGenerator
{
    /// <summary>
    /// Produz um código único, repetindo a geração caso já exista (camada 1 da unicidade).
    /// </summary>
    Task<string> GenerateUniqueAsync(CancellationToken ct = default);
}
