using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Sessions;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;

namespace EventGate.Api.Application.Services;

/// <summary>
/// Presença por palestra. Regras de negócio:
/// - a pessoa precisa ter validado na entrada principal do evento (Status=CheckedIn);
/// - o código tem que ser do mesmo evento da palestra;
/// - não dá pra marcar a mesma palestra duas vezes.
/// Casos esperados devolvem Success=false (não lançam), como na portaria principal.
/// </summary>
public sealed class SessionCheckInService(
    IRegistrationRepository registrations,
    ISessionRepository sessions,
    ISessionAttendanceRepository attendances,
    IAuditLogger audit)
{
    public async Task<SessionAttendResponse> AttendAsync(
        Guid sessionId,
        SessionAttendRequest request,
        Guid performedByUserId,
        CancellationToken ct = default)
    {
        var session = await sessions.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("Palestra não encontrada.");

        var registration = await registrations.GetByCodeAsync(request.AccessCode.Trim(), ct);

        if (registration is null)
        {
            await audit.LogAsync("SessionRejected", session.EventId, null, performedByUserId,
                $"Código inexistente (palestra {sessionId}).", ct);
            return new SessionAttendResponse(false, "Código inválido.", SessionTitle: session.Title);
        }

        if (registration.EventId != session.EventId)
        {
            await audit.LogAsync("SessionRejected", session.EventId, registration.Id, performedByUserId,
                "Código de outro evento.", ct);
            return new SessionAttendResponse(false, "Código é de outro evento.",
                registration.ParticipantName, session.Title);
        }

        if (registration.Status != RegistrationStatus.CheckedIn)
        {
            await audit.LogAsync("SessionRejected", session.EventId, registration.Id, performedByUserId,
                "Sem check-in na entrada principal.", ct);
            return new SessionAttendResponse(false, "Faça o check-in na entrada principal primeiro.",
                registration.ParticipantName, session.Title);
        }

        if (await attendances.ExistsAsync(sessionId, registration.Id, ct))
        {
            await audit.LogAsync("SessionRejected", session.EventId, registration.Id, performedByUserId,
                "Já registrado nesta palestra.", ct);
            return new SessionAttendResponse(false, "Já registrado nesta palestra.",
                registration.ParticipantName, session.Title, AlreadyAttended: true);
        }

        await attendances.AddAsync(new SessionAttendance
        {
            SessionId = sessionId,
            RegistrationId = registration.Id,
            CheckedInByUserId = performedByUserId
        }, ct);

        await audit.LogAsync("SessionCheckIn", session.EventId, registration.Id, performedByUserId,
            $"Presença na palestra: {session.Title}.", ct);

        return new SessionAttendResponse(true, "Presença confirmada.",
            registration.ParticipantName, session.Title);
    }
}
