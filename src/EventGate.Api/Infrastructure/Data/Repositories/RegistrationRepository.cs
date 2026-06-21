using EventGate.Api.Application.Dtos.Dashboard;
using EventGate.Api.Application.Dtos.Registrations;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure.Data.Repositories;

public sealed class RegistrationRepository(AppDbContext db) : IRegistrationRepository
{
    public Task<Registration?> GetByCodeAsync(string accessCode, CancellationToken ct = default) =>
        db.Registrations.Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.AccessCode == accessCode, ct);

    public Task<Registration?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Registrations.Include(r => r.Course).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> CodeExistsAsync(string accessCode, CancellationToken ct = default) =>
        db.Registrations.AnyAsync(r => r.AccessCode == accessCode, ct);

    public async Task AddAsync(Registration registration, CancellationToken ct = default)
    {
        db.Registrations.Add(registration);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Registration registration, CancellationToken ct = default)
    {
        // Direito ao esquecimento (LGPD): apaga as presenças em palestras antes,
        // pois a FK é NoAction (evita múltiplos caminhos de cascade no SQL Server).
        await db.SessionAttendances.Where(a => a.RegistrationId == registration.Id).ExecuteDeleteAsync(ct);
        db.Registrations.Remove(registration);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<int> CountByStatusAsync(Guid eventId, RegistrationStatus status, CancellationToken ct = default) =>
        db.Registrations.CountAsync(r => r.EventId == eventId && r.Status == status, ct);

    public Task<int> CountActiveAsync(Guid eventId, CancellationToken ct = default) =>
        db.Registrations.CountAsync(
            r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled, ct);

    public async Task<IReadOnlyList<RegistrationListItem>> ListByEventAsync(Guid eventId, string? search, CancellationToken ct = default)
    {
        var query = db.Registrations.AsNoTracking()
            .Include(r => r.Course)
            .Where(r => r.EventId == eventId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.ParticipantName.Contains(search) || r.ParticipantEmail.Contains(search));
        }

        var rows = await query
            .OrderBy(r => r.ParticipantName)
            .Select(r => new
            {
                r.Id,
                r.ParticipantName,
                r.ParticipantEmail,
                CourseName = r.Course != null ? r.Course.Name : r.CourseOther,
                r.Semester,
                r.Status,
                r.CreatedAt,
                r.CheckedInAt
            })
            .ToListAsync(ct);

        return rows.Select(x => new RegistrationListItem(
            x.Id, x.ParticipantName, x.ParticipantEmail, x.CourseName ?? "—",
            x.Semester, x.Status.ToString(), x.CreatedAt, x.CheckedInAt)).ToList();
    }

    public async Task<IReadOnlyList<CourseStat>> GetCourseStatsAsync(Guid eventId, CancellationToken ct = default)
    {
        var known = await db.Registrations.AsNoTracking()
            .Where(r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled && r.CourseId != null)
            .GroupBy(r => r.Course!.Name)
            .Select(g => new CourseStat(
                g.Key,
                g.Count(),
                g.Count(x => x.Status == RegistrationStatus.CheckedIn)))
            .ToListAsync(ct);

        var result = known.OrderByDescending(c => c.Registered).ToList();

        var otherRegistered = await db.Registrations.CountAsync(
            r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled && r.CourseId == null, ct);

        if (otherRegistered > 0)
        {
            var otherCheckedIn = await db.Registrations.CountAsync(
                r => r.EventId == eventId && r.CourseId == null && r.Status == RegistrationStatus.CheckedIn, ct);
            result.Add(new CourseStat("Outros/Externos", otherRegistered, otherCheckedIn));
        }

        return result;
    }

    public async Task<IReadOnlyList<SemesterStat>> GetSemesterStatsAsync(Guid eventId, CancellationToken ct = default)
    {
        var bySemester = await db.Registrations.AsNoTracking()
            .Where(r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled && r.Semester != null)
            .GroupBy(r => r.Semester!.Value)
            .Select(g => new { Semester = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var result = bySemester
            .OrderBy(s => s.Semester)
            .Select(s => new SemesterStat(s.Semester.ToString(), s.Count))
            .ToList();

        var noSemester = await db.Registrations.CountAsync(
            r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled && r.Semester == null, ct);

        if (noSemester > 0)
        {
            result.Add(new SemesterStat("Sem semestre", noSemester));
        }

        return result;
    }
}
