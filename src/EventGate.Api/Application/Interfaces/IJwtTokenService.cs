using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Gera um JWT assinado para o usuário, com perfil e expiração.</summary>
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user);
}
