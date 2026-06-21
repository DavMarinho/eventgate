using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Registrations;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Application.Services;
using EventGate.Api.Domain.Entities;
using Moq;
using Xunit;

namespace EventGate.Tests.Services;

public class RegistrationServiceTests
{
    private readonly Mock<IEventRepository> _events = new();
    private readonly Mock<IRegistrationRepository> _registrations = new();
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly Mock<IAccessCodeGenerator> _codeGen = new();
    private readonly Mock<IQrCodeGenerator> _qr = new();
    private readonly Mock<IEmailQueue> _emailQueue = new();

    private static readonly byte[] Photo = [1, 2, 3, 4];
    private const string PhotoType = "image/jpeg";

    private RegistrationService CreateSut() =>
        new(_events.Object, _registrations.Object, _courses.Object, _codeGen.Object, _qr.Object, _emailQueue.Object);

    private static Event SampleEvent(int capacity = 10) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Show",
        StartsAt = DateTimeOffset.UtcNow.AddDays(1),
        Capacity = capacity,
        OrganizerId = Guid.NewGuid()
    };

    private static CreateRegistrationRequest Request(
        Guid? courseId = null, string? courseOther = "Externo", int? semester = null, bool consent = true) =>
        new("Ana", "Ana@X.com", new DateOnly(2000, 1, 1), courseId, courseOther, semester, consent);

    [Fact]
    public async Task RegisterAsync_Throws_WhenConsentNotAccepted()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateSut().RegisterAsync(Guid.NewGuid(), Request(consent: false), Photo, PhotoType));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenPhotoMissing()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateSut().RegisterAsync(Guid.NewGuid(), Request(), [], PhotoType));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenBothCourseIdAndOther()
    {
        var req = Request(courseId: Guid.NewGuid(), courseOther: "Externo");
        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateSut().RegisterAsync(Guid.NewGuid(), req, Photo, PhotoType));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenNoCourseAtAll()
    {
        var req = Request(courseId: null, courseOther: null);
        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateSut().RegisterAsync(Guid.NewGuid(), req, Photo, PhotoType));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenCourseFromListWithoutValidSemester()
    {
        var courseId = Guid.NewGuid();
        _courses.Setup(c => c.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = courseId, Name = "Direito" });

        var req = Request(courseId: courseId, courseOther: null, semester: null);
        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateSut().RegisterAsync(Guid.NewGuid(), req, Photo, PhotoType));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEventNotFound()
    {
        _events.Setup(e => e.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateSut().RegisterAsync(Guid.NewGuid(), Request(), Photo, PhotoType));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEventFull()
    {
        var ev = SampleEvent(capacity: 2);
        _events.Setup(e => e.GetByIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ev);
        _registrations.Setup(r => r.CountActiveAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateSut().RegisterAsync(ev.Id, Request(), Photo, PhotoType));
    }

    [Fact]
    public async Task RegisterAsync_Succeeds_WithCourseFromList_AndQueuesEmail()
    {
        var ev = SampleEvent();
        var courseId = Guid.NewGuid();
        _events.Setup(e => e.GetByIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ev);
        _registrations.Setup(r => r.CountActiveAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _courses.Setup(c => c.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = courseId, Name = "Direito" });
        _codeGen.Setup(c => c.GenerateUniqueAsync(It.IsAny<CancellationToken>())).ReturnsAsync("ABCD2345");
        _qr.Setup(q => q.GeneratePng("ABCD2345")).Returns([9, 9, 9]);

        var req = Request(courseId: courseId, courseOther: null, semester: 3);
        var result = await CreateSut().RegisterAsync(ev.Id, req, Photo, PhotoType);

        Assert.Equal("ABCD2345", result.AccessCode);
        Assert.Equal("Direito", result.Course);
        Assert.Equal(3, result.Semester);
        Assert.Equal("ana@x.com", result.ParticipantEmail);
        _registrations.Verify(r => r.AddAsync(
            It.Is<Registration>(x => x.AccessCode == "ABCD2345" && x.CourseId == courseId && x.Semester == 3),
            It.IsAny<CancellationToken>()), Times.Once);
        _emailQueue.Verify(q => q.Enqueue(It.Is<EmailMessage>(m => m.ToEmail == "ana@x.com")), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Succeeds_WithCourseOther_NoSemester()
    {
        var ev = SampleEvent();
        _events.Setup(e => e.GetByIdAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ev);
        _registrations.Setup(r => r.CountActiveAsync(ev.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _codeGen.Setup(c => c.GenerateUniqueAsync(It.IsAny<CancellationToken>())).ReturnsAsync("ZZZZ2345");
        _qr.Setup(q => q.GeneratePng(It.IsAny<string>())).Returns([1]);

        var req = Request(courseOther: "Faculdade X", semester: 5);
        var result = await CreateSut().RegisterAsync(ev.Id, req, Photo, PhotoType);

        Assert.Equal("Faculdade X", result.Course);
        Assert.Null(result.Semester); // "Outro" ignora semestre
    }

    [Fact]
    public async Task GetOwnDataAsync_Throws_WhenEmailDoesNotMatchCode()
    {
        var reg = new Registration
        {
            EventId = Guid.NewGuid(),
            ParticipantName = "Ana",
            ParticipantEmail = "ana@x.com",
            PhotoData = Photo,
            PhotoContentType = PhotoType,
            AccessCode = "ABCD2345",
            CourseOther = "Externo"
        };
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        var request = new AccessByCodeRequest { AccessCode = "ABCD2345", Email = "outro@x.com" };
        await Assert.ThrowsAsync<NotFoundException>(() => CreateSut().GetOwnDataAsync(request));
    }

    [Fact]
    public async Task DeleteOwnDataAsync_Removes_WhenCodeAndEmailMatch()
    {
        var reg = new Registration
        {
            EventId = Guid.NewGuid(),
            ParticipantName = "Ana",
            ParticipantEmail = "ana@x.com",
            PhotoData = Photo,
            PhotoContentType = PhotoType,
            AccessCode = "ABCD2345",
            CourseOther = "Externo"
        };
        _registrations.Setup(r => r.GetByCodeAsync("ABCD2345", It.IsAny<CancellationToken>())).ReturnsAsync(reg);

        var request = new AccessByCodeRequest { AccessCode = "ABCD2345", Email = "Ana@X.com" };
        await CreateSut().DeleteOwnDataAsync(request);

        _registrations.Verify(r => r.RemoveAsync(reg, It.IsAny<CancellationToken>()), Times.Once);
    }
}
