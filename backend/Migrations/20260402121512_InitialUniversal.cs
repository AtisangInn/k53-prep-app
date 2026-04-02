using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K53PrepApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialUniversal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(nullable: false),
                    SubCategory = table.Column<string>(nullable: false),
                    Text = table.Column<string>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    OptionA = table.Column<string>(nullable: false),
                    OptionB = table.Column<string>(nullable: false),
                    OptionC = table.Column<string>(nullable: false),
                    OptionD = table.Column<string>(nullable: false),
                    CorrectOption = table.Column<string>(nullable: false),
                    Explanation = table.Column<string>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    FirstSeen = table.Column<DateTime>(nullable: false),
                    LastSeen = table.Column<DateTime>(nullable: false),
                    FlippedCardsCount = table.Column<int>(nullable: false),
                    TotalStudySeconds = table.Column<int>(nullable: false),
                    FreeTestsUsed = table.Column<int>(nullable: false),
                    FreeFlipsToday = table.Column<int>(nullable: false),
                    FreeNextsToday = table.Column<int>(nullable: false),
                    LastFreeFlipDate = table.Column<DateTime>(nullable: true),
                    DeviceId = table.Column<string>(nullable: false),
                    IpAddress = table.Column<string>(nullable: false),
                    IsPremium = table.Column<bool>(nullable: false),
                    PremiumUntil = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentPayments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(nullable: false),
                    MPaymentId = table.Column<string>(nullable: false),
                    PfPaymentId = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CompletedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPayments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(nullable: false),
                    TakenAt = table.Column<DateTime>(nullable: false),
                    DurationSeconds = table.Column<int>(nullable: false),
                    RulesScore = table.Column<int>(nullable: false),
                    RulesTotal = table.Column<int>(nullable: false),
                    SignsScore = table.Column<int>(nullable: false),
                    SignsTotal = table.Column<int>(nullable: false),
                    ControlsScore = table.Column<int>(nullable: false),
                    ControlsTotal = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResults_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TestResultId = table.Column<int>(nullable: false),
                    QuestionId = table.Column<int>(nullable: false),
                    ChosenOption = table.Column<string>(nullable: true),
                    IsCorrect = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestAnswers_TestResults_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "TestResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentPayments_StudentId",
                table: "StudentPayments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Name_Phone",
                table: "Students",
                columns: new[] { "Name", "Phone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswers_QuestionId",
                table: "TestAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswers_TestResultId",
                table: "TestAnswers",
                column: "TestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_StudentId",
                table: "TestResults",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentPayments");

            migrationBuilder.DropTable(
                name: "TestAnswers");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
