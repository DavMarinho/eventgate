using EventGate.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventGate.Api.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Speaker> Speakers => Set<Speaker>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionAttendance> SessionAttendances => Set<SessionAttendance>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasConversion<int>();
        });

        modelBuilder.Entity<Event>(e =>
        {
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.Name).HasMaxLength(200).IsRequired();
            e.Property(ev => ev.Description).HasMaxLength(2000);
            e.Property(ev => ev.Location).HasMaxLength(300);
            e.HasOne(ev => ev.Organizer)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(ev => ev.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Course>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Code).HasMaxLength(50);
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Registration>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.ParticipantName).HasMaxLength(150).IsRequired();
            e.Property(r => r.ParticipantEmail).HasMaxLength(256).IsRequired();
            e.Property(r => r.PhotoData).IsRequired();
            e.Property(r => r.PhotoContentType).HasMaxLength(100).IsRequired();
            e.Property(r => r.CourseOther).HasMaxLength(200);
            e.Property(r => r.AccessCode).HasMaxLength(32).IsRequired();
            e.Ignore(r => r.CourseDisplay);

            // Camada 2 da unicidade do código: índice único no banco.
            e.HasIndex(r => r.AccessCode).IsUnique();
            e.Property(r => r.Status).HasConversion<int>();

            e.HasOne(r => r.Event)
                .WithMany(ev => ev.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Course)
                .WithMany(c => c.Registrations)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Speaker>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).HasMaxLength(150).IsRequired();
            e.Property(s => s.Role).HasMaxLength(150);
            e.Property(s => s.Talk).HasMaxLength(200);
            e.Property(s => s.Bio).HasMaxLength(1000);
            e.Property(s => s.PhotoUrl).HasMaxLength(500);
            e.HasOne(s => s.Event)
                .WithMany()
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Title).HasMaxLength(200).IsRequired();
            e.Property(s => s.Speaker).HasMaxLength(150);
            e.Property(s => s.Room).HasMaxLength(100);
            e.HasOne(s => s.Event)
                .WithMany()
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SessionAttendance>(e =>
        {
            e.HasKey(a => a.Id);
            // Um registro por pessoa por palestra.
            e.HasIndex(a => new { a.SessionId, a.RegistrationId }).IsUnique();
            e.HasOne(a => a.Session)
                .WithMany()
                .HasForeignKey(a => a.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Registration)
                .WithMany()
                .HasForeignKey(a => a.RegistrationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(100).IsRequired();
            e.Property(a => a.Detail).HasMaxLength(1000);
            e.HasIndex(a => a.Timestamp);
        });
    }
}
