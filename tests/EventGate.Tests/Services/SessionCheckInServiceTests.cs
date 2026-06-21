using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Sessions;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Application.Services;
using EventGate.Api.Domain.Entities;
using EventGate.Api.Domain.Enums;
using Moq;
using Xunit;

namespace EventGate.Tests.Services;

public class SessionCheckInServiceTests
{
    private readonly Mock<IRegistrationRepository> _registrations = new();
    private readonly Mock<ISessionRepository> _sessions = new();
    private readonly Mock<ISessionAttendanceRepository> _attendances = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Guid _staffId = Guid.NewGuid();
    private readonly Guid _eventId = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();

    private SessionCheckInService CreateSut() =>
        new(_registrations.Object, _sessions.Object, _attendances.Object, _audit.Object);

    private void SetupSession() =>
        _sessions.Setup(s => s.GetByIdAsync(_sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Session { Id = _sessionId, EventId = _eventId, Title = "IA", StartsAt = DateTimeOffset.UtcNow });

    private Registration Reg(RegistrationStatus status, Guid? eventId = null) => new()
    {
        EventId = eventId ?? _eventId,
        ParticipantName = "Ana",
        ParticipantEmail = "ana@x.com",
        PhotoData = [1],
        PhotoContentType = "image/png",
        AccessCode = "ABCD2345",
        Status = status
    };

    private SessionAttendRequest Request() => new() { AccessCode = "ABCD2345" };

    [Fact]
    public async Task AttendAsync_Throws_WhenSessionMissing()
    {
        _sessions.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().AttendAsync(_sessionId, Request(), _staffId));
    }

    [Fact]
    public async Task AttendAsync_Fails_WhenNotCheckedInAtMainGate()
    {
        SetupSession();
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Reg(RegistrationStatus.Registered));

        var result = await CreateSut().AttendAsync(_sessionId, Request(), _staffId);

        Assert.False(result.Success);
        Assert.Contains("entrada principal", result.Reason);
        _attendances.Verify(a => a.AddAsync(It.IsAny<SessionAttendance>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AttendAsync_Fails_WhenDifferentEvent()
    {
        SetupSession();
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Reg(RegistrationStatus.CheckedIn, eventId: Guid.NewGuid()));

        var result = await CreateSut().AttendAsync(_sessionId, Request(), _staffId);

        Assert.False(result.Success);
        Assert.Contains("outro evento", result.Reason);
    }

    [Fact]
    public async Task AttendAsync_Fails_WhenAlreadyAttended()
    {
        SetupSession();
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Reg(RegistrationStatus.CheckedIn));
        _attendances.Setup(a => a.ExistsAsync(_sessionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().AttendAsync(_sessionId, Request(), _staffId);

        Assert.False(result.Success);
        Assert.True(result.AlreadyAttended);
    }

    [Fact]
    public async Task AttendAsync_RecordsAttendance_OnSuccess()
    {
        SetupSession();
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Reg(RegistrationStatus.CheckedIn));
        _attendances.Setup(a => a.ExistsAsync(_sessionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateSut().AttendAsync(_sessionId, Request(), _staffId);

        Assert.True(result.Success);
        _attendances.Verify(a => a.AddAsync(
            It.Is<SessionAttendance>(x => x.SessionId == _sessionId && x.CheckedInByUserId == _staffId),
            It.IsAny<CancellationToken>()), Times.Once);
        _audit.Verify(a => a.LogAsync("SessionCheckIn",
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), _staffId, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
