using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Interfaces;

public interface ISessionAttendanceRepository
{
    Task<bool> ExistsAsync(Guid sessionId, Guid registrationId, CancellationToken ct = default);
    Task AddAsync(SessionAttendance attendance, CancellationToken ct = default);
}
