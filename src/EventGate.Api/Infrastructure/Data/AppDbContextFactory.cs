using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventGate.Api.Infrastructure.Data;

/// <summary>
/// Fábrica usada SÓ em tempo de design (dotnet ef migrations). Evita que as
/// ferramentas executem o Program (que faz seed/Migrate e tentaria conectar ao
/// banco). A connection string aqui é apenas um placeholder — gerar migrations
/// não abre conexão.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=EventGate;Trusted_Connection=False;TrustServerCertificate=True")
            .Options;

        return new AppDbContext(options);
    }
}
