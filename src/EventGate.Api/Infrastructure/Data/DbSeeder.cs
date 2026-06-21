using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure.Data;

/// <summary>
/// Aplica migrations pendentes e cria os dados iniciais (organizador + cursos USP)
/// se não existirem. Credenciais de desenvolvimento — troque antes de uso real.
/// </summary>
public static class DbSeeder
{
    public const string SeedAdminEmail = "admin@eventgate.local";
    public const string SeedAdminPassword = "Admin@123";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // SQLite (dev) não usa as migrations do SQL Server — cria o schema direto.
        if (db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            await db.Database.EnsureCreatedAsync(ct);
        }
        else
        {
            await db.Database.MigrateAsync(ct);
        }

        if (!await db.Users.AnyAsync(ct))
        {
            db.Users.Add(new User
            {
                Email = SeedAdminEmail,
                PasswordHash = hasher.Hash(SeedAdminPassword),
                Role = UserRole.Organizer
            });
        }

        if (!await db.Courses.AnyAsync(ct))
        {
            db.Courses.AddRange(UspCourses.Select(name => new Course { Name = name }));
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Conjunto representativo de cursos de graduação da USP. Como o evento é
    /// aberto, a inscrição também aceita "Outro" (texto livre); a equipe pode
    /// adicionar novos cursos via POST /api/courses.
    /// </summary>
    private static readonly string[] UspCourses =
    [
        "Administração",
        "Arquitetura e Urbanismo",
        "Artes Cênicas",
        "Artes Visuais",
        "Audiovisual",
        "Biblioteconomia e Documentação",
        "Biotecnologia",
        "Ciência da Computação",
        "Ciências Biológicas",
        "Ciências Biomédicas",
        "Ciências Contábeis",
        "Ciências Econômicas",
        "Ciências Sociais",
        "Design",
        "Direito",
        "Educação Física e Esporte",
        "Enfermagem",
        "Engenharia Ambiental",
        "Engenharia Civil",
        "Engenharia de Computação",
        "Engenharia de Materiais",
        "Engenharia de Produção",
        "Engenharia Elétrica",
        "Engenharia Mecânica",
        "Engenharia Mecatrônica",
        "Engenharia Química",
        "Estatística",
        "Farmácia e Bioquímica",
        "Filosofia",
        "Física",
        "Fisioterapia",
        "Fonoaudiologia",
        "Geografia",
        "Geologia",
        "Gerontologia",
        "Gestão Ambiental",
        "Gestão de Políticas Públicas",
        "História",
        "Jornalismo",
        "Lazer e Turismo",
        "Letras",
        "Marketing",
        "Matemática",
        "Matemática Aplicada",
        "Medicina",
        "Medicina Veterinária",
        "Meteorologia",
        "Música",
        "Nutrição",
        "Obstetrícia",
        "Oceanografia",
        "Odontologia",
        "Pedagogia",
        "Psicologia",
        "Publicidade e Propaganda",
        "Química",
        "Relações Internacionais",
        "Relações Públicas",
        "Sistemas de Informação",
        "Terapia Ocupacional",
        "Têxtil e Moda",
        "Turismo",
        "Zootecnia"
    ];
}
