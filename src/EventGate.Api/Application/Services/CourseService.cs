using EventGate.Api.Application.Common;
using EventGate.Api.Application.Dtos.Courses;
using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Services;

public sealed class CourseService(ICourseRepository courses)
{
    public async Task<IReadOnlyList<CourseResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await courses.GetAllActiveAsync(ct);
        return list.Select(c => new CourseResponse(c.Id, c.Name, c.Code)).ToList();
    }

    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (await courses.ExistsByNameAsync(name, ct))
        {
            throw new ConflictException("Já existe um curso com este nome.");
        }

        var course = new Course { Name = name, Code = request.Code?.Trim() };
        await courses.AddAsync(course, ct);

        return new CourseResponse(course.Id, course.Name, course.Code);
    }
}
