using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Registrations;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;

namespace EventGate.Api.Application.Services;

/// <summary>
/// Inscrição pública (com foto + curso), autoatendimento LGPD e listagem para a
/// equipe. Após salvar, gera o QR e enfileira o e-mail de confirmação (assíncrono).
/// </summary>
public sealed class RegistrationService(
    IEventRepository events,
    IRegistrationRepository registrations,
    ICourseRepository courses,
    IAccessCodeGenerator codeGenerator,
    IQrCodeGenerator qrCode,
    IEmailQueue emailQueue)
{
    private static readonly string[] AllowedPhotoTypes = ["image/jpeg", "image/png"];
    private const int MaxPhotoBytes = 3 * 1024 * 1024; // 3 MB

    public async Task<RegistrationResponse> RegisterAsync(
        Guid eventId,
        CreateRegistrationRequest data,
        byte[] photo,
        string photoContentType,
        CancellationToken ct = default)
    {
        if (!data.ConsentAccepted)
        {
            throw new ValidationException("É obrigatório aceitar o consentimento (LGPD) para se inscrever.");
        }

        ValidatePhoto(photo, photoContentType);

        var (course, semester) = await ResolveCourseAsync(data, ct);

        var @event = await events.GetByIdAsync(eventId, ct)
            ?? throw new NotFoundException("Evento não encontrado.");

        var active = await registrations.CountActiveAsync(eventId, ct);
        if (active >= @event.Capacity)
        {
            throw new ConflictException("Evento lotado.");
        }

        var code = await codeGenerator.GenerateUniqueAsync(ct);

        var registration = new Registration
        {
            EventId = eventId,
            ParticipantName = data.ParticipantName.Trim(),
            ParticipantEmail = data.ParticipantEmail.Trim().ToLowerInvariant(),
            PhotoData = photo,
            PhotoContentType = photoContentType,
            BirthDate = data.BirthDate,
            CourseId = course?.Id,
            CourseOther = course is null ? data.CourseOther?.Trim() : null,
            Semester = semester,
            AccessCode = code,
            Status = RegistrationStatus.Registered,
            ConsentAccepted = true,
            ConsentAcceptedAt = DateTimeOffset.UtcNow
        };

        await registrations.AddAsync(registration, ct);

        QueueConfirmationEmail(registration, @event, course?.Name ?? data.CourseOther);

        return ToResponse(registration, course?.Name ?? registration.CourseOther ?? "—");
    }

    public async Task<RegistrationResponse> GetOwnDataAsync(AccessByCodeRequest request, CancellationToken ct = default)
    {
        var registration = await FindOwnedAsync(request, ct);
        return ToResponse(registration, registration.CourseDisplay);
    }

    public async Task DeleteOwnDataAsync(AccessByCodeRequest request, CancellationToken ct = default)
    {
        var registration = await FindOwnedAsync(request, ct);
        await registrations.RemoveAsync(registration, ct);
    }

    /// <summary>Lista de inscritos do evento, para o painel da equipe.</summary>
    public async Task<IReadOnlyList<RegistrationListItem>> ListAsync(Guid eventId, string? search, CancellationToken ct = default)
    {
        _ = await events.GetByIdAsync(eventId, ct)
            ?? throw new NotFoundException("Evento não encontrado.");

        return await registrations.ListByEventAsync(eventId, search?.Trim(), ct);
    }

    /// <summary>Foto da inscrição (uso interno do controller, staff-only).</summary>
    public async Task<(byte[] Data, string ContentType)> GetPhotoAsync(Guid registrationId, CancellationToken ct = default)
    {
        var registration = await registrations.GetByIdAsync(registrationId, ct)
            ?? throw new NotFoundException("Inscrição não encontrada.");

        return (registration.PhotoData, registration.PhotoContentType);
    }

    private async Task<(Course? Course, int? Semester)> ResolveCourseAsync(CreateRegistrationRequest data, CancellationToken ct)
    {
        var hasCourseId = data.CourseId.HasValue;
        var hasOther = !string.IsNullOrWhiteSpace(data.CourseOther);

        if (hasCourseId == hasOther)
        {
            throw new ValidationException("Informe um curso da lista OU um curso em \"Outro\" (apenas um dos dois).");
        }

        if (hasCourseId)
        {
            var course = await courses.GetByIdAsync(data.CourseId!.Value, ct)
                ?? throw new ValidationException("Curso inválido.");

            if (data.Semester is not (>= 1 and <= 12))
            {
                throw new ValidationException("Semestre deve estar entre 1 e 12.");
            }

            return (course, data.Semester);
        }

        // "Outro" não tem semestre.
        return (null, null);
    }

    private static void ValidatePhoto(byte[] photo, string contentType)
    {
        if (photo.Length == 0)
        {
            throw new ValidationException("A foto é obrigatória.");
        }

        if (photo.Length > MaxPhotoBytes)
        {
            throw new ValidationException("A foto excede o tamanho máximo de 3 MB.");
        }

        if (!AllowedPhotoTypes.Contains(contentType))
        {
            throw new ValidationException("Formato de foto inválido. Use JPG ou PNG.");
        }
    }

    private void QueueConfirmationEmail(Registration registration, Event @event, string? courseName)
    {
        var qrPng = qrCode.GeneratePng(registration.AccessCode);
        var html = BuildEmailHtml(registration, @event, courseName);

        emailQueue.Enqueue(new EmailMessage(
            ToEmail: registration.ParticipantEmail,
            ToName: registration.ParticipantName,
            Subject: $"Sua inscrição em {@event.Name}",
            HtmlContent: html,
            AttachmentBytes: qrPng,
            AttachmentName: "qrcode.png"));
    }

    private static string BuildEmailHtml(Registration registration, Event @event, string? courseName)
    {
        var when = @event.StartsAt.ToString("dd/MM/yyyy 'às' HH:mm");
        var local = string.IsNullOrWhiteSpace(@event.Location) ? "" :
            $"""<tr><td style="padding:4px 0;color:#6b7280">Local</td><td style="padding:4px 0;text-align:right">{@event.Location}</td></tr>""";

        return $"""
            <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#111827">
              <div style="background:#1a1d27;color:#fff;padding:20px 24px;border-radius:12px 12px 0 0">
                <h1 style="margin:0;font-size:22px">Event<span style="color:#5b8cff">Gate</span></h1>
              </div>
              <div style="border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px;padding:24px">
                <h2 style="margin:0 0 8px;font-size:18px">Inscrição confirmada 🎉</h2>
                <p style="margin:0 0 16px">Olá, {registration.ParticipantName}! Sua inscrição foi confirmada.</p>
                <table style="width:100%;font-size:14px;border-collapse:collapse;margin-bottom:16px">
                  <tr><td style="padding:4px 0;color:#6b7280">Evento</td><td style="padding:4px 0;text-align:right"><strong>{@event.Name}</strong></td></tr>
                  <tr><td style="padding:4px 0;color:#6b7280">Quando</td><td style="padding:4px 0;text-align:right">{when}</td></tr>
                  {local}
                  <tr><td style="padding:4px 0;color:#6b7280">Curso</td><td style="padding:4px 0;text-align:right">{courseName ?? "—"}</td></tr>
                </table>
                <p style="margin:0 0 8px">Apresente este código (ou o QR Code anexo) na portaria:</p>
                <div style="background:#f3f4f6;border:2px dashed #5b8cff;border-radius:12px;text-align:center;padding:16px;font-size:30px;font-weight:bold;letter-spacing:6px;color:#1a1d27">{registration.AccessCode}</div>
                <p style="margin:16px 0 0;font-size:13px;color:#6b7280">Guarde este e-mail. O código é pessoal e intransferível.</p>
              </div>
            </div>
            """;
    }

    private async Task<Registration> FindOwnedAsync(AccessByCodeRequest request, CancellationToken ct)
    {
        var registration = await registrations.GetByCodeAsync(request.AccessCode.Trim(), ct);

        var email = request.Email.Trim().ToLowerInvariant();
        if (registration is null ||
            !string.Equals(registration.ParticipantEmail, email, StringComparison.Ordinal))
        {
            throw new NotFoundException("Inscrição não encontrada para o código e e-mail informados.");
        }

        return registration;
    }

    private static RegistrationResponse ToResponse(Registration r, string courseDisplay) => new(
        r.Id,
        r.EventId,
        r.ParticipantName,
        r.ParticipantEmail,
        r.AccessCode,
        r.Status.ToString(),
        courseDisplay,
        r.Semester,
        r.BirthDate,
        r.ConsentAccepted,
        r.ConsentAcceptedAt,
        r.CreatedAt,
        r.CheckedInAt);
}
