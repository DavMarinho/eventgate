using EventGate.Api.Application.Dtos.Dashboard;
using EventGate.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventGate.Api.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Organizer,Validator")]
public sealed class DashboardController(DashboardService dashboard) : ControllerBase
{
    /// <summary>Participação por curso (inscritos x presentes).</summary>
    [HttpGet("events/{eventId:guid}/by-course")]
    [ProducesResponseType(typeof(IReadOnlyList<CourseStat>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CourseStat>>> ByCourse(Guid eventId, CancellationToken ct)
        => Ok(await dashboard.ByCourseAsync(eventId, ct));

    /// <summary>Distribuição por semestre.</summary>
    [HttpGet("events/{eventId:guid}/by-semester")]
    [ProducesResponseType(typeof(IReadOnlyList<SemesterStat>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SemesterStat>>> BySemester(Guid eventId, CancellationToken ct)
        => Ok(await dashboard.BySemesterAsync(eventId, ct));

    /// <summary>Presença por palestra.</summary>
    [HttpGet("events/{eventId:guid}/by-session")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionStat>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SessionStat>>> BySession(Guid eventId, CancellationToken ct)
        => Ok(await dashboard.BySessionAsync(eventId, ct));
}
