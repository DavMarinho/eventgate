using EventGate.Api.Application.Dtos.CheckIn;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Application.Services;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;
using Moq;
using Xunit;

namespace EventGate.Tests.Services;

public class CheckInServiceTests
{
    private readonly Mock<IRegistrationRepository> _registrations = new();
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Guid _staffId = Guid.NewGuid();

    private CheckInService CreateSut() => new(_registrations.Object, _events.Object, _audit.Object);

    private static Registration SampleRegistration(RegistrationStatus status = RegistrationStatus.Registered) => new()
    {
        EventId = Guid.NewGuid(),
        ParticipantName = "Ana",
        ParticipantEmail = "ana@x.com",
        PhotoData = [1, 2, 3],
        PhotoContentType = "image/png",
        AccessCode = "ABCD2345",
        CourseOther = "Externo",
        Status = status
    };

    [Fact]
    public async Task LookupAsync_ReturnsData_WithPhoto_AndAudits()
    {
        var reg = SampleRegistration();
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        var result = await CreateSut().LookupAsync("ABCD2345", _staffId);

        Assert.True(result.Found);
        Assert.Equal("Ana", result.ParticipantName);
        Assert.StartsWith("data:image/png;base64,", result.PhotoDataUri);
        Assert.False(result.AlreadyCheckedIn);
        _audit.Verify(a => a.LogAsync("CheckInLookup",
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), _staffId, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LookupAsync_ReturnsNotFound_WhenCodeUnknown()
    {
        _registrations.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration?)null);

        var result = await CreateSut().LookupAsync("NOPE", _staffId);

        Assert.False(result.Found);
        Assert.Null(result.PhotoDataUri);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalid_AndAudits_WhenCodeUnknown()
    {
        _registrations.Setup(r => r.GetByCodeAsync("NOPE", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration?)null);

        var result = await CreateSut().ValidateAsync(new ValidateCodeRequest { AccessCode = "NOPE" }, _staffId);

        Assert.False(result.Valid);
        _audit.Verify(a => a.LogAsync("CheckInRejected",
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), _staffId, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_RejectsReuse_WhenAlreadyCheckedIn()
    {
        var reg = SampleRegistration(RegistrationStatus.CheckedIn);
        reg.CheckedInAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        var result = await CreateSut().ValidateAsync(new ValidateCodeRequest { AccessCode = "ABCD2345" }, _staffId);

        Assert.False(result.Valid);
        Assert.Equal("Código já utilizado.", result.Reason);
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_MarksCheckedIn_AndAudits_OnSuccess()
    {
        var reg = SampleRegistration();
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        var result = await CreateSut().ValidateAsync(new ValidateCodeRequest { AccessCode = "ABCD2345" }, _staffId);

        Assert.True(result.Valid);
        Assert.Equal(RegistrationStatus.CheckedIn, reg.Status);
        Assert.NotNull(reg.CheckedInAt);
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _audit.Verify(a => a.LogAsync("CheckInValidated",
            reg.EventId, reg.Id, _staffId, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_ComputesPending_FromTotalMinusCheckedIn()
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Show",
            StartsAt = DateTimeOffset.UtcNow.AddDays(1),
            Capacity = 100,
            OrganizerId = Guid.NewGuid()
        };
        _events.Setup(e => e.GetByIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ev);
        _registrations.Setup(r => r.CountActiveAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(30);
        _registrations.Setup(r => r.CountByStatusAsync(ev.Id, RegistrationStatus.CheckedIn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);

        var result = await CreateSut().GetStatsAsync(ev.Id);

        Assert.Equal(30, result.TotalRegistered);
        Assert.Equal(12, result.CheckedIn);
        Assert.Equal(18, result.Pending);
        Assert.Equal(100, result.Capacity);
    }
}
