using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K53PrepApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class IroncladProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questions",
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FlippedCardsCount = table.Column<int>(nullable: false),
                    TotalStudySeconds = table.Column<int>(nullable: false),
                    FreeTestsUsed = table.Column<int>(nullable: false),
                    FreeFlipsToday = table.Column<int>(nullable: false),
                    FreeNextsToday = table.Column<int>(nullable: false),
                    LastFreeFlipDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeviceId = table.Column<string>(nullable: false),
                    IpAddress = table.Column<string>(nullable: false),
                    IsPremium = table.Column<bool>(nullable: false),
                    PremiumUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "studentpayments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(nullable: false),
                    MPaymentId = table.Column<string>(nullable: false),
                    PfPaymentId = table.Column<string>(nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_studentpayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_studentpayments_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "testresults",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(nullable: false),
                    TakenAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_testresults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_testresults_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "testanswers",
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
                    table.PrimaryKey("PK_testanswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_testanswers_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_testanswers_testresults_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "testresults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_studentpayments_StudentId",
                table: "studentpayments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_students_Name_Phone",
                table: "students",
                columns: new[] { "Name", "Phone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_testanswers_QuestionId",
                table: "testanswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_testanswers_TestResultId",
                table: "testanswers",
                column: "TestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_testresults_StudentId",
                table: "testresults",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "studentpayments");

            migrationBuilder.DropTable(
                name: "testanswers");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "testresults");

            migrationBuilder.DropTable(
                name: "students");
        }
    }
}
