using EventGate.Api.Application.Dtos.Sessions;
using EventGate.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventGate.Api.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class SessionsController(SessionService sessions) : ControllerBase
{
    /// <summary>Palestras de um evento (público — usado na programação da landing).</summary>
    [HttpGet("events/{eventId:guid}/sessions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<SessionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> List(Guid eventId, CancellationToken ct)
        => Ok(await sessions.ListByEventAsync(eventId, ct));

    /// <summary>Cria uma palestra (somente Organizer).</summary>
    [HttpPost("events/{eventId:guid}/sessions")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionResponse>> Create(Guid eventId, CreateSessionRequest request, CancellationToken ct)
    {
        var response = await sessions.CreateAsync(eventId, request, ct);
        return CreatedAtAction(nameof(List), new { eventId }, response);
    }

    /// <summary>Remove uma palestra (somente Organizer).</summary>
    [HttpDelete("sessions/{id:guid}")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sessions.DeleteAsync(id, ct);
        return NoContent();
    }
}
