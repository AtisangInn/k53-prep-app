using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K53PrepApp.Api.Migrations
{
    public partial class FixBooleanColumnTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Force lowercase table boolean conversion (Primary Target)
            migrationBuilder.Sql("ALTER TABLE \"questions\" ALTER COLUMN \"IsActive\" TYPE boolean USING CASE WHEN \"IsActive\" = 0 THEN false ELSE true END;");
            migrationBuilder.Sql("ALTER TABLE \"students\" ALTER COLUMN \"IsPremium\" TYPE boolean USING CASE WHEN \"IsPremium\" = 0 THEN false ELSE true END;");
            migrationBuilder.Sql("ALTER TABLE \"testanswers\" ALTER COLUMN \"IsCorrect\" TYPE boolean USING CASE WHEN \"IsCorrect\" = 0 THEN false ELSE true END;");

            // 2. Force PascalCase table boolean conversion (Ghost Targets)
            migrationBuilder.Sql("ALTER TABLE \"Questions\" ALTER COLUMN \"IsActive\" TYPE boolean USING CASE WHEN \"IsActive\" = 0 THEN false ELSE true END;");
            migrationBuilder.Sql("ALTER TABLE \"Students\" ALTER COLUMN \"IsPremium\" TYPE boolean USING CASE WHEN \"IsPremium\" = 0 THEN false ELSE true END;");
            migrationBuilder.Sql("ALTER TABLE \"TestAnswers\" ALTER COLUMN \"IsCorrect\" TYPE boolean USING CASE WHEN \"IsCorrect\" = 0 THEN false ELSE true END;");
            
            // 3. Manually seed the migration history so EF doesn't try to re-run Everything and crash
            migrationBuilder.Sql("INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20260402161940_DefinitivePostgresStandard', '8.0.0') ON CONFLICT DO NOTHING;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert logic
            migrationBuilder.Sql("ALTER TABLE \"questions\" ALTER COLUMN \"IsActive\" TYPE integer USING CASE WHEN \"IsActive\" = false THEN 0 ELSE 1 END;");
            migrationBuilder.Sql("ALTER TABLE \"students\" ALTER COLUMN \"IsPremium\" TYPE integer USING CASE WHEN \"IsPremium\" = false THEN 0 ELSE 1 END;");
            migrationBuilder.Sql("ALTER TABLE \"testanswers\" ALTER COLUMN \"IsCorrect\" TYPE integer USING CASE WHEN \"IsCorrect\" = false THEN 0 ELSE 1 END;");
        }
    }
}
