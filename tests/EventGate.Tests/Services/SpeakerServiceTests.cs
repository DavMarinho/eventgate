using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Speakers;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Application.Services;
using EventGate.Api.Domain.Entities;
using Moq;
using Xunit;

namespace EventGate.Tests.Services;

public class SpeakerServiceTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<ISpeakerRepository> _speakers = new();

    private SpeakerService CreateSut() => new(_events.Object, _speakers.Object);

    private static Event SampleEvent() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Show",
        StartsAt = DateTimeOffset.UtcNow.AddDays(1),
        Capacity = 10,
        OrganizerId = Guid.NewGuid()
    };

    [Fact]
    public async Task CreateAsync_Throws_WhenEventNotFound()
    {
        _events.Setup(e => e.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().CreateAsync(Guid.NewGuid(), new CreateSpeakerRequest { Name = "Ana" }));
    }

    [Fact]
    public async Task CreateAsync_Adds_AndReturns()
    {
        var ev = SampleEvent();
        _events.Setup(e => e.GetByIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ev);

        var req = new CreateSpeakerRequest { Name = "  Ana  ", Role = "USP", Talk = "IA", PhotoUrl = "" };
        var result = await CreateSut().CreateAsync(ev.Id, req);

        Assert.Equal("Ana", result.Name);
        Assert.Null(result.PhotoUrl); // string vazia vira null
        _speakers.Verify(s => s.AddAsync(
            It.Is<Speaker>(x => x.Name == "Ana" && x.EventId == ev.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenNotFound()
    {
        _speakers.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Speaker?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateSut().DeleteAsync(Guid.NewGuid()));
    }
}
