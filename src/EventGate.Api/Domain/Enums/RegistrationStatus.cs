namespace EventGate.Api.Domain.Enums;

/// <summary>
/// Ciclo de vida de uma inscrição.
/// </summary>
public enum RegistrationStatus
{
    /// <summary>Inscrito, ainda não validado na portaria.</summary>
    Registered = 1,

    /// <summary>Já entrou no evento. Bloqueia reuso do código.</summary>
    CheckedIn = 2,

    /// <summary>Cancelada (ex.: titular exerceu o direito ao esquecimento).</summary>
    Cancelled = 3
}
