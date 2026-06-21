using EventGate.Api.Application.Dtos.CheckIn;
using EventGate.Api.Application.Dtos.Sessions;
using EventGate.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventGate.Api.Api.Controllers;

[ApiController]
[Route("api/checkin")]
[Authorize(Roles = "Organizer,Validator")]
public sealed class CheckInController(CheckInService checkIn, SessionCheckInService sessionCheckIn) : ControllerBase
{
    /// <summary>Passo 1: lê o código e devolve os dados para conferência (foto inclusa). Não consome.</summary>
    [HttpGet("lookup")]
    [EnableRateLimiting("public")]
    [ProducesResponseType(typeof(GateLookupResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GateLookupResponse>> Lookup([FromQuery] string code, CancellationToken ct)
    {
        var userId = User.GetUserId();
        return Ok(await checkIn.LookupAsync(code, userId, ct));
    }

    /// <summary>Entrada principal — passo 2: confirma a entrada no evento e impede o reuso.</summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateCodeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidateCodeResponse>> Validate(ValidateCodeRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        return Ok(await checkIn.ValidateAsync(request, userId, ct));
    }

    /// <summary>Porta de uma palestra: marca presença na palestra (exige check-in na entrada principal).</summary>
    [HttpPost("sessions/{sessionId:guid}/attend")]
    [ProducesResponseType(typeof(SessionAttendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionAttendResponse>> Attend(Guid sessionId, SessionAttendRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        return Ok(await sessionCheckIn.AttendAsync(sessionId, request, userId, ct));
    }

    /// <summary>Estatísticas de presença de um evento (entrada principal).</summary>
    [HttpGet("events/{eventId:guid}/stats")]
    [ProducesResponseType(typeof(CheckInStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckInStatsResponse>> Stats(Guid eventId, CancellationToken ct)
        => Ok(await checkIn.GetStatsAsync(eventId, ct));
}
