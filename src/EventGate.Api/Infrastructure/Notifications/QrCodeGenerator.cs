using EventGate.Api.Application.Interfaces;
using QRCoder;

namespace EventGate.Api.Infrastructure.Notifications;

/// <summary>
/// Gera o PNG do QR Code com QRCoder. Usa <see cref="PngByteQRCode"/>, que não
/// depende de System.Drawing — funciona em Linux (Docker) sem libs nativas.
/// </summary>
public sealed class QrCodeGenerator : IQrCodeGenerator
{
    public byte[] GeneratePng(string content)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(20);
    }
}
