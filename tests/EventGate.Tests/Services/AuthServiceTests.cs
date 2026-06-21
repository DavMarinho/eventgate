using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Auth;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Application.Services;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;
using Moq;
using Xunit;

namespace EventGate.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();

    private AuthService CreateSut() => new(_users.Object, _hasher.Object, _jwt.Object);

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsValid()
    {
        var user = new User { Email = "a@b.com", PasswordHash = "hash", Role = UserRole.Organizer };
        _users.Setup(u => u.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("pw", "hash")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns(("token", DateTimeOffset.UtcNow.AddHours(1)));

        var result = await CreateSut().LoginAsync(new LoginRequest { Email = "a@b.com", Password = "pw" });

        Assert.Equal("token", result.AccessToken);
        Assert.Equal("Organizer", result.Role);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserNotFound()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<AuthException>(() =>
            CreateSut().LoginAsync(new LoginRequest { Email = "x@y.com", Password = "pw" }));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenPasswordWrong()
    {
        var user = new User { Email = "a@b.com", PasswordHash = "hash" };
        _users.Setup(u => u.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        await Assert.ThrowsAsync<AuthException>(() =>
            CreateSut().LoginAsync(new LoginRequest { Email = "a@b.com", Password = "bad" }));
    }

    [Fact]
    public async Task RegisterStaffAsync_Throws_WhenEmailTaken()
    {
        _users.Setup(u => u.ExistsByEmailAsync("a@b.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new RegisterStaffRequest { Email = "a@b.com", Password = "password1", Role = UserRole.Validator };

        await Assert.ThrowsAsync<ConflictException>(() => CreateSut().RegisterStaffAsync(request));
    }

    [Fact]
    public async Task RegisterStaffAsync_HashesPassword_AndPersists()
    {
        _users.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _hasher.Setup(h => h.Hash("password1")).Returns("hashed");

        var request = new RegisterStaffRequest { Email = "New@B.com", Password = "password1", Role = UserRole.Validator };

        var result = await CreateSut().RegisterStaffAsync(request);

        Assert.Equal("new@b.com", result.Email); // normalizado para minúsculas
        _users.Verify(u => u.AddAsync(
            It.Is<User>(x => x.PasswordHash == "hashed" && x.Email == "new@b.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
