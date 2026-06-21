namespace EventGate.Api.Application.Dtos.Auth;

/// <summary>Conta de equipe (sem qualquer dado de senha).</summary>
public sealed record StaffResponse(Guid Id, string Email, string Role, DateTimeOffset CreatedAt);
