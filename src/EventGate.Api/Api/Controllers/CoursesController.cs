using EventGate.Api.Application.Dtos.Courses;
using EventGate.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventGate.Api.Api.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(CourseService courses) : ControllerBase
{
    /// <summary>Lista os cursos ativos (para o autocomplete da inscrição).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CourseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CourseResponse>>> GetAll(CancellationToken ct)
        => Ok(await courses.GetAllAsync(ct));

    /// <summary>Cria um curso (somente Organizer).</summary>
    [HttpPost]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(CourseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseResponse>> Create(CreateCourseRequest request, CancellationToken ct)
    {
        var response = await courses.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { id = response.Id }, response);
    }
}
