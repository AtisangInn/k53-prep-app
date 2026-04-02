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
        // Force all tables to lowercase for absolute PostgreSQL compatibility
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var currentTableName = entity.GetTableName();
            if (currentTableName != null)
                entity.SetTableName(currentTableName.ToLower());

            // Force native PostgreSQL types for dates, decimals, and booleans
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp without time zone");
                }
                else if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetColumnType("numeric");
                }
                else if (property.ClrType == typeof(bool) || property.ClrType == typeof(bool?))
                {
                    property.SetColumnType("boolean");
                }
            }
        }

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
