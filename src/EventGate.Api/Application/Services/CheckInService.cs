using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.CheckIn;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Enums;

namespace EventGate.Api.Application.Services;

/// <summary>
/// Portaria: leitura (lookup) para conferência e validação (impede reuso).
/// Toda tentativa vira trilha de auditoria. Código inválido/reusado não lança
/// erro — devolve um resultado negativo, evitando pistas a quem enumera códigos.
/// </summary>
public sealed class CheckInService(
    IRegistrationRepository registrations,
    IEventRepository events,
    IAuditLogger audit)
{
    /// <summary>Passo 1: dados para a equipe conferir a pessoa. Não consome o código.</summary>
    public async Task<GateLookupResponse> LookupAsync(
        string accessCode,
        Guid performedByUserId,
        CancellationToken ct = default)
    {
        var code = accessCode.Trim();
        var registration = await registrations.GetByCodeAsync(code, ct);

        if (registration is null)
        {
            await audit.LogAsync("CheckInLookup", performedByUserId: performedByUserId,
                detail: "Código inexistente.", ct: ct);
            return new GateLookupResponse(false, "Código inválido.");
        }

        await audit.LogAsync("CheckInLookup", registration.EventId, registration.Id, performedByUserId,
            "Consulta na portaria.", ct);

        var photoUri = $"data:{registration.PhotoContentType};base64,{Convert.ToBase64String(registration.PhotoData)}";

        return new GateLookupResponse(
            Found: true,
            Reason: registration.Status switch
            {
                RegistrationStatus.CheckedIn => "Atenção: código já utilizado.",
                RegistrationStatus.Cancelled => "Atenção: inscrição cancelada.",
                _ => "Inscrição válida."
            },
            RegistrationId: registration.Id,
            ParticipantName: registration.ParticipantName,
            Course: registration.CourseDisplay,
            Semester: registration.Semester,
            BirthDate: registration.BirthDate,
            Email: registration.ParticipantEmail,
            Status: registration.Status.ToString(),
            AlreadyCheckedIn: registration.Status == RegistrationStatus.CheckedIn,
            CheckedInAt: registration.CheckedInAt,
            PhotoDataUri: photoUri);
    }

    /// <summary>Passo 2: confirma a entrada e impede o reuso.</summary>
    public async Task<ValidateCodeResponse> ValidateAsync(
        ValidateCodeRequest request,
        Guid performedByUserId,
        CancellationToken ct = default)
    {
        var code = request.AccessCode.Trim();
        var registration = await registrations.GetByCodeAsync(code, ct);

        if (registration is null)
        {
            await audit.LogAsync("CheckInRejected", performedByUserId: performedByUserId,
                detail: "Código inexistente.", ct: ct);
            return new ValidateCodeResponse(false, "Código inválido.");
        }

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            await audit.LogAsync("CheckInRejected", registration.EventId, registration.Id, performedByUserId,
                "Inscrição cancelada.", ct);
            return new ValidateCodeResponse(false, "Inscrição cancelada.");
        }

        if (registration.Status == RegistrationStatus.CheckedIn)
        {
            await audit.LogAsync("CheckInRejected", registration.EventId, registration.Id, performedByUserId,
                "Reuso de código (já validado).", ct);
            return new ValidateCodeResponse(false, "Código já utilizado.",
                registration.ParticipantName, registration.EventId, registration.CheckedInAt);
        }

        registration.Status = RegistrationStatus.CheckedIn;
        registration.CheckedInAt = DateTimeOffset.UtcNow;
        await registrations.SaveChangesAsync(ct);

        await audit.LogAsync("CheckInValidated", registration.EventId, registration.Id, performedByUserId,
            "Entrada confirmada.", ct);

        return new ValidateCodeResponse(true, "Entrada confirmada.",
            registration.ParticipantName, registration.EventId, registration.CheckedInAt);
    }

    public async Task<CheckInStatsResponse> GetStatsAsync(Guid eventId, CancellationToken ct = default)
    {
        var @event = await events.GetByIdAsync(eventId, ct)
            ?? throw new NotFoundException("Evento não encontrado.");

        var total = await registrations.CountActiveAsync(eventId, ct);
        var checkedIn = await registrations.CountByStatusAsync(eventId, RegistrationStatus.CheckedIn, ct);

        return new CheckInStatsResponse(
            eventId,
            @event.Capacity,
            total,
            checkedIn,
            Pending: total - checkedIn);
    }
}
