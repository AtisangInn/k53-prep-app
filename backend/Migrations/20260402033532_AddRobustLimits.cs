using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K53PrepApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRobustLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FreeNextsToday",
                table: "Students",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FreeNextsToday",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Students");
        }
    }
}
