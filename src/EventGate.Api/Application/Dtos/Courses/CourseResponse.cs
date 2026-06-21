namespace EventGate.Api.Application.Dtos.Courses;

/// <summary>Curso para popular o autocomplete da inscrição.</summary>
public sealed record CourseResponse(Guid Id, string Name, string? Code);
