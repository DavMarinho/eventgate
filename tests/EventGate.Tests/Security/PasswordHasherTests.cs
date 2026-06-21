using EventGate.Api.Infrastructure.Security;
using Xunit;

namespace EventGate.Tests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesDifferentOutputForSamePassword_DueToRandomSalt()
    {
        var a = _hasher.Hash("Secret@123");
        var b = _hasher.Hash("Secret@123");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var hash = _hasher.Hash("Secret@123");

        Assert.True(_hasher.Verify("Secret@123", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hash = _hasher.Hash("Secret@123");

        Assert.False(_hasher.Verify("wrong", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-valid-format")]
    [InlineData("100000.notbase64.notbase64")]
    public void Verify_ReturnsFalse_ForMalformedStoredHash(string storedHash)
    {
        Assert.False(_hasher.Verify("anything", storedHash));
    }
}
