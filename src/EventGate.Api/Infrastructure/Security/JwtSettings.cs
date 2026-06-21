namespace EventGate.Api.Infrastructure.Security;

/// <summary>
/// Configuração do JWT, vinda da seção "Jwt" do appsettings.
/// A chave (Key) é um placeholder de desenvolvimento — troque em produção
/// e mantenha fora do controle de versão (User Secrets / Key Vault).
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "EventGate";
    public string Audience { get; set; } = "EventGate";
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
