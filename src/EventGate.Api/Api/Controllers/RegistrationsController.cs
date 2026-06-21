using EventGate.Api.Api.Models;
using EventGate.Api.Application.Dtos.Registrations;
using EventGate.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventGate.Api.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class RegistrationsController(RegistrationService registrations) : ControllerBase
{
    /// <summary>Inscrição pública (multipart, com foto). Gera código + QR e dispara o e-mail.</summary>
    [HttpPost("events/{eventId:guid}/registrations")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [RequestSizeLimit(4_000_000)]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponse>> Register(
        Guid eventId,
        [FromForm] CreateRegistrationForm form,
        CancellationToken ct)
    {
        await using var stream = new MemoryStream();
        await form.Photo.CopyToAsync(stream, ct);

        var data = new CreateRegistrationRequest(
            form.ParticipantName,
            form.ParticipantEmail,
            form.BirthDate,
            form.CourseId,
            form.CourseOther,
            form.Semester,
            form.ConsentAccepted);

        var response = await registrations.RegisterAsync(eventId, data, stream.ToArray(), form.Photo.ContentType, ct);
        return CreatedAtAction(nameof(GetMyData), null, response);
    }

    /// <summary>Foto da inscrição (somente equipe). Nunca pública.</summary>
    [HttpGet("registrations/{id:guid}/photo")]
    [Authorize(Roles = "Organizer,Validator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Photo(Guid id, CancellationToken ct)
    {
        var (bytes, contentType) = await registrations.GetPhotoAsync(id, ct);
        return File(bytes, contentType);
    }

    /// <summary>Lista de inscritos de um evento, com busca (somente equipe).</summary>
    [HttpGet("registrations/events/{eventId:guid}")]
    [Authorize(Roles = "Organizer,Validator")]
    [ProducesResponseType(typeof(IReadOnlyList<RegistrationListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RegistrationListItem>>> List(
        Guid eventId,
        [FromQuery] string? search,
        CancellationToken ct)
        => Ok(await registrations.ListAsync(eventId, search, ct));

    /// <summary>LGPD — direito de acesso: o titular consulta os próprios dados (código + e-mail no corpo).</summary>
    [HttpPost("registrations/me")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationResponse>> GetMyData(AccessByCodeRequest request, CancellationToken ct)
        => Ok(await registrations.GetOwnDataAsync(request, ct));

    /// <summary>LGPD — direito ao esquecimento: o titular solicita a exclusão.</summary>
    [HttpDelete("registrations/me")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMyData(AccessByCodeRequest request, CancellationToken ct)
    {
        await registrations.DeleteOwnDataAsync(request, ct);
        return NoContent();
    }
}
