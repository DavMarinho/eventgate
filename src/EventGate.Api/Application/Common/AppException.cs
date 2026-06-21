namespace EventGate.Api.Application.Common;

/// <summary>
/// Exceção de aplicação com código HTTP associado. O middleware global de erros
/// traduz para uma resposta segura (sem vazar stack trace ou detalhes internos).
/// </summary>
public abstract class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

/// <summary>Recurso não encontrado (404).</summary>
public sealed class NotFoundException(string message) : AppException(message, StatusCodes.Status404NotFound);

/// <summary>Conflito de estado, ex.: e-mail já usado ou lotação esgotada (409).</summary>
public sealed class ConflictException(string message) : AppException(message, StatusCodes.Status409Conflict);

/// <summary>Entrada inválida segundo regra de negócio (400).</summary>
public sealed class ValidationException(string message) : AppException(message, StatusCodes.Status400BadRequest);

/// <summary>Credenciais inválidas (401). Mensagem mantida genérica de propósito.</summary>
public sealed class AuthException(string message) : AppException(message, StatusCodes.Status401Unauthorized);
