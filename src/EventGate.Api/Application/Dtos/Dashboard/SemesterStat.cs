namespace EventGate.Api.Application.Dtos.Dashboard;

/// <summary>Distribuição por semestre. "Label" pode ser "1".."12" ou "Sem semestre".</summary>
public sealed record SemesterStat(string Label, int Registered);
