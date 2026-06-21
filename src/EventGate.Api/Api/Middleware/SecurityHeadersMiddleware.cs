namespace EventGate.Api.Api.Middleware;

/// <summary>
/// Adiciona cabeçalhos de segurança HTTP em todas as respostas.
/// Como a API não serve HTML, a CSP é bem restritiva (default-src 'none').
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // Remove o cabeçalho que expõe o servidor, quando presente.
        headers.Remove("Server");

        await next(context);
    }
}
