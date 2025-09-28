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
        public DbSet<LiveRoom> LiveRooms { get; set; } = null!;
        public DbSet<LiveRoomPlayer> LiveRoomPlayers { get; set; } = null!;
        public DbSet<LiveRoomAnswer> LiveRoomAnswers { get; set; } = null!;


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

            modelBuilder.Entity<LiveRoom>(b =>
            {
                b.HasKey(lr => lr.Id);

                b.HasIndex(lr => lr.RoomCode)
                 .IsUnique();

                b.HasOne(lr => lr.Quiz)
                 .WithMany()
                 .HasForeignKey(lr => lr.QuizId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.Property(lr => lr.RoomCode)
                 .IsRequired()
                 .HasMaxLength(8);

                b.Property(lr => lr.Name)
                 .IsRequired()
                 .HasMaxLength(100);

                b.Property(lr => lr.CurrentQuestionIndex)
                 .HasDefaultValue(-1);

                b.Property(lr => lr.IsActive)
                 .HasDefaultValue(true);
            });

            modelBuilder.Entity<LiveRoomPlayer>(b =>
            {
                b.HasKey(lrp => lrp.Id);

                b.HasIndex(lrp => lrp.UserId)
                    .IsUnique()
                    .HasFilter("[LeftAt] IS NULL");

                b.HasOne(lrp => lrp.LiveRoom)
                    .WithMany(lr => lr.Players)
                    .HasForeignKey(lrp => lrp.LiveRoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(lrp => lrp.User)
                    .WithMany()
                    .HasForeignKey(lrp => lrp.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(); // ✅ ensure every LiveRoomPlayer must have a User
            });


            modelBuilder.Entity<LiveRoomAnswer>(b =>
            {
                b.HasKey(lra => lra.Id);

                b.HasIndex(lra => new { lra.LiveRoomId, lra.UserId, lra.QuestionId })
                 .IsUnique();

                b.HasOne(lra => lra.LiveRoom)
                 .WithMany(lr => lr.Answers)
                 .HasForeignKey(lra => lra.LiveRoomId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(lra => lra.User)
                 .WithMany()
                 .HasForeignKey(lra => lra.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(lra => lra.Question)
                 .WithMany()
                 .HasForeignKey(lra => lra.QuestionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.Property(lra => lra.SubmittedAnswer)
                 .IsRequired()
                 .HasMaxLength(500);
            });
        }
    }
}