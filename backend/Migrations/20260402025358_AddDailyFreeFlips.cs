using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K53PrepApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyFreeFlips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FreeFlipsToday",
                table: "Students",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFreeFlipDate",
                table: "Students",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeFlipsToday",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LastFreeFlipDate",
                table: "Students");
        }
    }
}
