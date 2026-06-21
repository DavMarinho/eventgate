using EventGate.Api.Application.Interfaces;
using EventGate.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure.Data.Repositories;

public sealed class CourseRepository(AppDbContext db) : ICourseRepository
{
    public async Task<IReadOnlyList<Course>> GetAllActiveAsync(CancellationToken ct = default) =>
        await db.Courses.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        db.Courses.AnyAsync(c => c.Name == name, ct);

    public async Task AddAsync(Course course, CancellationToken ct = default)
    {
        db.Courses.Add(course);
        await db.SaveChangesAsync(ct);
    }
}
