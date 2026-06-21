using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Courses;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Application.Services;
using EventGate.Api.Domain.Entities;
using Moq;
using Xunit;

namespace EventGate.Tests.Services;

public class CourseServiceTests
{
    private readonly Mock<ICourseRepository> _courses = new();

    private CourseService CreateSut() => new(_courses.Object);

    [Fact]
    public async Task GetAllAsync_MapsToResponse()
    {
        _courses.Setup(c => c.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Course { Name = "Direito" }, new Course { Name = "Medicina" }]);

        var result = await CreateSut().GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Direito", result[0].Name);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenNameExists()
    {
        _courses.Setup(c => c.ExistsByNameAsync("Direito", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateSut().CreateAsync(new CreateCourseRequest { Name = "Direito" }));
    }

    [Fact]
    public async Task CreateAsync_Adds_WhenNew()
    {
        _courses.Setup(c => c.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateSut().CreateAsync(new CreateCourseRequest { Name = "  Física  " });

        Assert.Equal("Física", result.Name);
        _courses.Verify(c => c.AddAsync(It.Is<Course>(x => x.Name == "Física"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
