using EventGate.Api.Infrastructure.Notifications;
using Xunit;

namespace EventGate.Tests.Security;

public class QrCodeGeneratorTests
{
    [Fact]
    public void GeneratePng_ReturnsValidPngBytes()
    {
        var generator = new QrCodeGenerator();

        var png = generator.GeneratePng("ABCD2345");

        Assert.NotEmpty(png);
        // Assinatura PNG: 137 80 78 71 (\x89 P N G)
        Assert.Equal(0x89, png[0]);
        Assert.Equal((byte)'P', png[1]);
        Assert.Equal((byte)'N', png[2]);
        Assert.Equal((byte)'G', png[3]);
    }
}
