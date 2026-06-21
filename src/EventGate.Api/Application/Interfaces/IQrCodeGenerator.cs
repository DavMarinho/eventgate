namespace EventGate.Api.Application.Interfaces;

/// <summary>Gera a imagem (PNG) de um QR Code a partir de um conteúdo textual.</summary>
public interface IQrCodeGenerator
{
    byte[] GeneratePng(string content);
}
