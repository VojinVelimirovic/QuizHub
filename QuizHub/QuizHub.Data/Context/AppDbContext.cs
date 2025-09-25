using Microsoft.EntityFrameworkCore;
using QuizHub.Data.Models;

namespace QuizHub.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<AnswerOption> AnswerOptions { get; set; } = null!;
        public DbSet<QuizResult> QuizResults { get; set; } = null!;
        public DbSet<QuizResultAnswer> QuizResultAnswers { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Category)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CategoryId);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId);

            modelBuilder.Entity<AnswerOption>()
                .HasOne(a => a.Question)
                .WithMany(q => q.AnswerOptions)
                .HasForeignKey(a => a.QuestionId);

            modelBuilder.Entity<QuizResult>()
                .HasOne(r => r.User)
                .WithMany(u => u.QuizResults)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<QuizResult>()
                .HasOne(r => r.Quiz)
                .WithMany(q => q.QuizResults)
                .HasForeignKey(r => r.QuizId);

            modelBuilder.Entity<QuizResultAnswer>(b =>
            {
                b.HasKey(a => a.Id);

                b.HasOne(a => a.QuizResult)
                 .WithMany(r => r.Answers)
                 .HasForeignKey(a => a.QuizResultId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(a => a.AnswerOption)
                 .WithMany()
                 .HasForeignKey(a => a.AnswerOptionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
