using EventGate.Api.Application.Interfaces;
using EventGate.Api.Application.Services;
using EventGate.Api.Infrastructure.Data;
using EventGate.Api.Infrastructure.Data.Repositories;
using EventGate.Api.Infrastructure.Notifications;
using EventGate.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure;

/// <summary>
/// Registro de dependências da Application + Infrastructure.
/// As camadas dependem de abstrações (interfaces), ligadas às implementações aqui.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddEventGateInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Provider do banco: "SqlServer" (padrão/produção) ou "Sqlite" (dev sem servidor).
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(string.IsNullOrWhiteSpace(connectionString)
                    ? "Data Source=eventgate.db"
                    : connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        // Repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ISpeakerRepository, SpeakerRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionAttendanceRepository, SessionAttendanceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Serviços de segurança
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAccessCodeGenerator, AccessCodeGenerator>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        // Notificações: QR + e-mail (Brevo) com fila assíncrona
        services.Configure<BrevoSettings>(configuration.GetSection(BrevoSettings.SectionName));
        services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();
        services.AddSingleton<IEmailQueue, EmailQueue>();
        services.AddHttpClient<IEmailSender, BrevoEmailSender>();
        services.AddHostedService<EmailDispatcher>();

        // Casos de uso (Application)
        services.AddScoped<AuthService>();
        services.AddScoped<EventService>();
        services.AddScoped<RegistrationService>();
        services.AddScoped<CheckInService>();
        services.AddScoped<CourseService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<SpeakerService>();
        services.AddScoped<SessionService>();
        services.AddScoped<SessionCheckInService>();

        return services;
    }
}
