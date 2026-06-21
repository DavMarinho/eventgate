using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventGate.Api.Api;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id do usuário autenticado a partir da claim "sub".</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(sub, out var id)
            ? id
            : throw new InvalidOperationException("Token sem identificador de usuário válido.");
    }
}
