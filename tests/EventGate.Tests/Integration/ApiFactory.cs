using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EventGate.Tests.Integration;

/// <summary>
/// Sobe a API de verdade em memória, usando SQLite (arquivo temporário) e
/// configuração de teste. O seed roda no startup (cria schema + admin + cursos).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbFile = $"eventgate-test-{Guid.NewGuid():N}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbFile}");
        builder.UseSetting("Jwt:Key", "TEST_KEY_at_least_32_chars_long_0123456789_abcdef");
        builder.UseSetting("Jwt:Issuer", "EventGate");
        builder.UseSetting("Jwt:Audience", "EventGate");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var f in new[] { _dbFile, $"{_dbFile}-shm", $"{_dbFile}-wal" })
        {
            try { if (File.Exists(f)) File.Delete(f); } catch { /* arquivo em uso; ignora */ }
        }
    }
}
