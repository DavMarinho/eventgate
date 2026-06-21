namespace EventGate.Api.Application.Dtos.Dashboard;

/// <summary>Participação por curso (inscritos x presentes).</summary>
public sealed record CourseStat(string Course, int Registered, int CheckedIn);
