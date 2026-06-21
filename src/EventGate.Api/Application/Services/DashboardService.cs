using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Dashboard;
using EventGate.Api.Application.Interfaces;

namespace EventGate.Api.Application.Services;

/// <summary>Agregações para o dashboard da equipe (feitas via SQL no repositório).</summary>
public sealed class DashboardService(
    IEventRepository events,
    IRegistrationRepository registrations,
    ISessionRepository sessions)
{
    public async Task<IReadOnlyList<CourseStat>> ByCourseAsync(Guid eventId, CancellationToken ct = default)
    {
        await EnsureEventAsync(eventId, ct);
        return await registrations.GetCourseStatsAsync(eventId, ct);
    }

    public async Task<IReadOnlyList<SemesterStat>> BySemesterAsync(Guid eventId, CancellationToken ct = default)
    {
        await EnsureEventAsync(eventId, ct);
        return await registrations.GetSemesterStatsAsync(eventId, ct);
    }

    public async Task<IReadOnlyList<SessionStat>> BySessionAsync(Guid eventId, CancellationToken ct = default)
    {
        await EnsureEventAsync(eventId, ct);
        return await sessions.GetStatsAsync(eventId, ct);
    }

    private async Task EnsureEventAsync(Guid eventId, CancellationToken ct)
    {
        _ = await events.GetByIdAsync(eventId, ct)
            ?? throw new NotFoundException("Evento não encontrado.");
    }
}
