using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using University.Models;

namespace University.Data;

public class UnivercityDbContext : IdentityDbContext<ApplicationUser>
{
    public UnivercityDbContext(DbContextOptions<UnivercityDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<GroupSubject> GroupSubjects => Set<GroupSubject>();
    public DbSet<StudentSubject> StudentSubjects => Set<StudentSubject>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Student>()
            .HasOne(s => s.User)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Lecturer>()
            .HasOne(l => l.User)
            .WithOne(u => u.Lecturer)
            .HasForeignKey<Lecturer>(l => l.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentSubject>()
            .HasKey(ss => new { ss.StudentId, ss.SubjectId });

        builder.Entity<StudentSubject>()
            .HasOne(ss => ss.Student)
            .WithMany(s => s.StudentSubjects)
            .HasForeignKey(ss => ss.StudentId);

        builder.Entity<StudentSubject>()
            .HasOne(ss => ss.Subject)
            .WithMany(su => su.StudentSubjects)
            .HasForeignKey(ss => ss.SubjectId);

        builder.Entity<GroupSubject>()
            .HasKey(gs => new { gs.StudentGroupId, gs.SubjectId });

        builder.Entity<GroupSubject>()
            .HasOne(gs => gs.StudentGroup)
            .WithMany(g => g.GroupSubjects)
            .HasForeignKey(gs => gs.StudentGroupId);

        builder.Entity<GroupSubject>()
            .HasOne(gs => gs.Subject)
            .WithMany(s => s.GroupSubjects)
            .HasForeignKey(gs => gs.SubjectId);

        builder.Entity<Student>()
            .HasOne(s => s.StudentGroup)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.StudentGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Subject>()
            .HasOne(s => s.Lecturer)
            .WithMany(l => l.Subjects)
            .HasForeignKey(s => s.LecturerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CalendarEvent>()
            .HasOne(e => e.OwnerUser)
            .WithMany()
            .HasForeignKey(e => e.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CalendarEvent>()
            .HasOne(e => e.Subject)
            .WithMany(s => s.CalendarEvents)
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
