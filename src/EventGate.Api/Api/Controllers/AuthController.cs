using EventGate.Api.Application.Dtos.Auth;
using EventGate.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventGate.Api.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>Login da equipe. Retorna um JWT (Bearer).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var response = await auth.LoginAsync(request, ct);
        return Ok(response);
    }

    /// <summary>Cria uma conta de equipe (somente Organizer).</summary>
    [HttpPost("register-staff")]
    [Authorize(Roles = "Organizer")]
    [ProducesResponseType(typeof(StaffResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffResponse>> RegisterStaff(RegisterStaffRequest request, CancellationToken ct)
    {
        var response = await auth.RegisterStaffAsync(request, ct);
        return CreatedAtAction(nameof(RegisterStaff), new { id = response.Id }, response);
    }
}
