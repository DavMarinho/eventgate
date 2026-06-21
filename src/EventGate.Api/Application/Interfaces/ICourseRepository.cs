using EventGate.Api.Domain.Entities;

namespace EventGate.Api.Application.Interfaces;

public interface ICourseRepository
{
    Task<IReadOnlyList<Course>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Course course, CancellationToken ct = default);
}
