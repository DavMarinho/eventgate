using EventGate.Api.Application.Dtos.Events;
using EventGate.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventGate.Api.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(EventService events) : ControllerBase
{
    /// <summary>Cria um evento (somente Organizer).</summary>
    [HttpPost]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<EventResponse>> Create(CreateEventRequest request, CancellationToken ct)
    {
        var organizerId = User.GetUserId();
        var response = await events.CreateAsync(request, organizerId, ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Lista os eventos (público).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<EventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>> GetAll(CancellationToken ct)
    {
        var response = await events.GetAllAsync(ct);
        return Ok(response);
    }

    /// <summary>Detalhe de um evento (público).</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetById(Guid id, CancellationToken ct)
    {
        var response = await events.GetByIdAsync(id, ct);
        return Ok(response);
    }
}
