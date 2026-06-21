using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Auth;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;

namespace EventGate.Api.Application.Services;

/// <summary>
/// Login da equipe e criação de contas. Depende apenas de abstrações
/// (repositório, hasher, serviço de token) — totalmente testável com mocks.
/// </summary>
public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwt)
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);

        // Mensagem genérica de propósito: não revela se o e-mail existe.
        // Verifica o hash mesmo quando o usuário não existe seria ideal contra
        // timing; aqui mantemos simples mas com mensagem única para ambos os casos.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthException("Credenciais inválidas.");
        }

        var (token, expiresAt) = jwt.GenerateToken(user);
        return new LoginResponse(token, expiresAt, user.Role.ToString());
    }

    public async Task<StaffResponse> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await users.ExistsByEmailAsync(email, ct))
        {
            throw new ConflictException("Já existe uma conta com este e-mail.");
        }

        if (!Enum.IsDefined(request.Role))
        {
            throw new ValidationException("Perfil inválido.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        await users.AddAsync(user, ct);

        return new StaffResponse(user.Id, user.Email, user.Role.ToString(), user.CreatedAt);
    }
}
