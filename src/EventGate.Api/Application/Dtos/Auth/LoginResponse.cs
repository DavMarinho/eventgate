namespace EventGate.Api.Application.Dtos.Auth;

/// <summary>Token JWT retornado após login bem-sucedido.</summary>
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, string Role);
