using EventGate.Api.Application.Dtos.Dashboard;
using EventGate.Api.Application.Dtos.Registrations;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;

namespace EventGate.Api.Application.Interfaces;

public interface IRegistrationRepository
{
    /// <summary>Busca por código já trazendo o curso (para lookup/exibição).</summary>
    Task<Registration?> GetByCodeAsync(string accessCode, CancellationToken ct = default);
    Task<Registration?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string accessCode, CancellationToken ct = default);
    Task AddAsync(Registration registration, CancellationToken ct = default);
    Task RemoveAsync(Registration registration, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    Task<int> CountByStatusAsync(Guid eventId, RegistrationStatus status, CancellationToken ct = default);
    Task<int> CountActiveAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>Lista de inscritos do evento, com busca opcional por nome/e-mail.</summary>
    Task<IReadOnlyList<RegistrationListItem>> ListByEventAsync(Guid eventId, string? search, CancellationToken ct = default);

    Task<IReadOnlyList<CourseStat>> GetCourseStatsAsync(Guid eventId, CancellationToken ct = default);
    Task<IReadOnlyList<SemesterStat>> GetSemesterStatsAsync(Guid eventId, CancellationToken ct = default);
}
