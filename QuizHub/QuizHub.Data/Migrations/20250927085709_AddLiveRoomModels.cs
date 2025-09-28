using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveRoomModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuizId = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    SecondsPerQuestion = table.Column<int>(type: "int", nullable: false),
                    StartDelaySeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CurrentQuestionIndex = table.Column<int>(type: "int", nullable: false, defaultValue: -1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveRooms_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveRoomAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LiveRoomId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SubmittedAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    GotFirstBlood = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveRoomAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveRoomAnswers_LiveRooms_LiveRoomId",
                        column: x => x.LiveRoomId,
                        principalTable: "LiveRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveRoomAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiveRoomAnswers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveRoomPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LiveRoomId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IsReady = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveRoomPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveRoomPlayers_LiveRooms_LiveRoomId",
                        column: x => x.LiveRoomId,
                        principalTable: "LiveRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveRoomPlayers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomAnswers_LiveRoomId_UserId_QuestionId",
                table: "LiveRoomAnswers",
                columns: new[] { "LiveRoomId", "UserId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomAnswers_QuestionId",
                table: "LiveRoomAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomAnswers_UserId",
                table: "LiveRoomAnswers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomPlayers_LiveRoomId",
                table: "LiveRoomPlayers",
                column: "LiveRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRoomPlayers_UserId",
                table: "LiveRoomPlayers",
                column: "UserId",
                unique: true,
                filter: "[LeftAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRooms_QuizId",
                table: "LiveRooms",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveRooms_RoomCode",
                table: "LiveRooms",
                column: "RoomCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveRoomAnswers");

            migrationBuilder.DropTable(
                name: "LiveRoomPlayers");

            migrationBuilder.DropTable(
                name: "LiveRooms");
        }
    }
}
