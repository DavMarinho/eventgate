using System.Security.Cryptography;
using EventGate.Api.Application.Interfaces;

namespace EventGate.Api.Infrastructure.Security;

/// <summary>
/// Gera o código de acesso com RNG criptograficamente seguro.
/// Alfabeto sem caracteres ambíguos (sem 0/O/1/I/L) para leitura na portaria.
/// Unicidade — camada 1: regenera se o código já existir (índice único = camada 2).
/// </summary>
public sealed class AccessCodeGenerator(IRegistrationRepository registrations) : IAccessCodeGenerator
{
    // Sem 0, O, 1, I, L para evitar confusão visual.
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 8;
    private const int MaxAttempts = 10;

    public async Task<string> GenerateUniqueAsync(CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var code = Generate();
            if (!await registrations.CodeExistsAsync(code, ct))
            {
                return code;
            }
        }

        // Improvável com 31^8 combinações; falha alto em vez de gerar colisão silenciosa.
        throw new InvalidOperationException("Não foi possível gerar um código de acesso único.");
    }

    private static string Generate()
    {
        var chars = new char[CodeLength];
        for (var i = 0; i < CodeLength; i++)
        {
            // GetInt32 usa RNG criptográfico e evita viés de módulo.
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(chars);
    }
}
