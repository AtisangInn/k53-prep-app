using Microsoft.EntityFrameworkCore;
using K53PrepApp.Models;

namespace K53PrepApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<TestAnswer> TestAnswers => Set<TestAnswer>();
    public DbSet<StudentPayment> StudentPayments => Set<StudentPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicitly map to lowercase table names for PostgreSQL compatibility
        modelBuilder.Entity<Student>().ToTable("students");
        modelBuilder.Entity<Question>().ToTable("questions");
        modelBuilder.Entity<TestResult>().ToTable("test_results");
        modelBuilder.Entity<TestAnswer>().ToTable("test_answers");
        modelBuilder.Entity<StudentPayment>().ToTable("student_payments");

        modelBuilder.Entity<Student>()
            .HasIndex(s => new { s.Name, s.Phone })
            .IsUnique();

        modelBuilder.Entity<TestResult>()
            .HasOne(t => t.Student)
            .WithMany(s => s.TestResults)
            .HasForeignKey(t => t.StudentId);

        modelBuilder.Entity<TestAnswer>()
            .HasOne(a => a.TestResult)
            .WithMany(t => t.Answers)
            .HasForeignKey(a => a.TestResultId);

        modelBuilder.Entity<TestAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId);

        modelBuilder.Entity<StudentPayment>()
            .HasOne(p => p.Student)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.StudentId);
    }
}
