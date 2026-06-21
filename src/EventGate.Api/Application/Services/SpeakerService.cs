using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Speakers;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Services;

public sealed class SpeakerService(IEventRepository events, ISpeakerRepository speakers)
{
    public async Task<IReadOnlyList<SpeakerResponse>> ListByEventAsync(Guid eventId, CancellationToken ct = default)
    {
        var list = await speakers.ListByEventAsync(eventId, ct);
        return list.Select(ToResponse).ToList();
    }

    public async Task<SpeakerResponse> CreateAsync(Guid eventId, CreateSpeakerRequest request, CancellationToken ct = default)
    {
        _ = await events.GetByIdAsync(eventId, ct)
            ?? throw new NotFoundException("Evento não encontrado.");

        var speaker = new Speaker
        {
            EventId = eventId,
            Name = request.Name.Trim(),
            Role = request.Role?.Trim(),
            Talk = request.Talk?.Trim(),
            Bio = request.Bio?.Trim(),
            PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim(),
            SortOrder = request.SortOrder
        };

        await speakers.AddAsync(speaker, ct);
        return ToResponse(speaker);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var speaker = await speakers.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Palestrante não encontrado.");

        await speakers.RemoveAsync(speaker, ct);
    }

    private static SpeakerResponse ToResponse(Speaker s) =>
        new(s.Id, s.EventId, s.Name, s.Role, s.Talk, s.Bio, s.PhotoUrl, s.SortOrder);
}
