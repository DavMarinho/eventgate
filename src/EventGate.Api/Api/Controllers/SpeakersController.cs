using EventGate.Api.Application.Dtos.Speakers;
using EventGate.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventGate.Api.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class SpeakersController(SpeakerService speakers) : ControllerBase
{
    /// <summary>Palestrantes de um evento (público — usado na landing).</summary>
    [HttpGet("events/{eventId:guid}/speakers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<SpeakerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SpeakerResponse>>> List(Guid eventId, CancellationToken ct)
        => Ok(await speakers.ListByEventAsync(eventId, ct));

    /// <summary>Adiciona um palestrante a um evento (somente Organizer).</summary>
    [HttpPost("events/{eventId:guid}/speakers")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(SpeakerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpeakerResponse>> Create(Guid eventId, CreateSpeakerRequest request, CancellationToken ct)
    {
        var response = await speakers.CreateAsync(eventId, request, ct);
        return CreatedAtAction(nameof(List), new { eventId }, response);
    }

    /// <summary>Remove um palestrante (somente Organizer).</summary>
    [HttpDelete("speakers/{id:guid}")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await speakers.DeleteAsync(id, ct);
        return NoContent();
    }
}
