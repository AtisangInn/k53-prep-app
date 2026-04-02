using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K53PrepApp.Api.Migrations
{
    public partial class FixBooleanColumnTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix questions.IsActive
            migrationBuilder.Sql("ALTER TABLE \"questions\" ALTER COLUMN \"IsActive\" TYPE boolean USING CASE WHEN \"IsActive\" = 0 THEN false ELSE true END;");

            // Fix students.IsPremium
            migrationBuilder.Sql("ALTER TABLE \"students\" ALTER COLUMN \"IsPremium\" TYPE boolean USING CASE WHEN \"IsPremium\" = 0 THEN false ELSE true END;");

            // Fix testanswers.IsCorrect
            migrationBuilder.Sql("ALTER TABLE \"testanswers\" ALTER COLUMN \"IsCorrect\" TYPE boolean USING CASE WHEN \"IsCorrect\" = 0 THEN false ELSE true END;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert questions.IsActive
            migrationBuilder.Sql("ALTER TABLE \"questions\" ALTER COLUMN \"IsActive\" TYPE integer USING CASE WHEN \"IsActive\" = false THEN 0 ELSE 1 END;");

            // Revert students.IsPremium
            migrationBuilder.Sql("ALTER TABLE \"students\" ALTER COLUMN \"IsPremium\" TYPE integer USING CASE WHEN \"IsPremium\" = false THEN 0 ELSE 1 END;");

            // Revert testanswers.IsCorrect
            migrationBuilder.Sql("ALTER TABLE \"testanswers\" ALTER COLUMN \"IsCorrect\" TYPE integer USING CASE WHEN \"IsCorrect\" = false THEN 0 ELSE 1 END;");
        }
    }
}
