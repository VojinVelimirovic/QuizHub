using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Quizzes");
        }
    }
}
