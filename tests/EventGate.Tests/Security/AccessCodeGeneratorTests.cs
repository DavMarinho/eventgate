using EventGate.Api.Application.Interfaces;
using EventGate.Api.Infrastructure.Security;
using Moq;
using Xunit;

namespace EventGate.Tests.Security;

public class AccessCodeGeneratorTests
{
    private readonly Mock<IRegistrationRepository> _repo = new();

    [Fact]
    public async Task GenerateUniqueAsync_ReturnsCode_WithoutAmbiguousChars()
    {
        _repo.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var generator = new AccessCodeGenerator(_repo.Object);

        var code = await generator.GenerateUniqueAsync();

        Assert.Equal(8, code.Length);
        Assert.DoesNotContain('0', code);
        Assert.DoesNotContain('O', code);
        Assert.DoesNotContain('1', code);
        Assert.DoesNotContain('I', code);
        Assert.DoesNotContain('L', code);
    }

    [Fact]
    public async Task GenerateUniqueAsync_Retries_WhenCodeAlreadyExists()
    {
        // Primeira tentativa colide, segunda é livre.
        _repo.SetupSequence(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var generator = new AccessCodeGenerator(_repo.Object);

        var code = await generator.GenerateUniqueAsync();

        Assert.NotNull(code);
        _repo.Verify(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GenerateUniqueAsync_Throws_WhenAllAttemptsCollide()
    {
        _repo.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var generator = new AccessCodeGenerator(_repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => generator.GenerateUniqueAsync());
    }
}
