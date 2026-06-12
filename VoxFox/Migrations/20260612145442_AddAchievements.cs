using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VoxFox.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Icon = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "Code", "Description", "Icon", "Title" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), "first_lesson", "Завершил свой первый урок", "🎯", "Первый шаг" },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), "lesson_5", "Завершил 5 уроков", "🔥", "На разгоне" },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), "lesson_10", "Завершил 10 уроков", "⚡", "В потоке" },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), "lesson_50", "Завершил 50 уроков", "🏃", "Марафонец" },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), "first_enrollment", "Записался на первый курс", "📚", "Студент" },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), "first_course", "Завершил свой первый курс", "🎓", "Выпускник" },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), "course_3", "Завершил 3 курса", "🏆", "Многостаночник" },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), "first_certificate", "Получил первый сертификат", "📜", "Дипломант" },
                    { new Guid("a1000000-0000-0000-0000-000000000009"), "first_review", "Оставил свой первый отзыв", "✍️", "Критик" },
                    { new Guid("a1000000-0000-0000-0000-000000000010"), "perfect_score", "Завершил курс с прогрессом 100%", "⭐", "Отличник" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_Code",
                table: "Achievements",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "Achievements");
        }
    }
}
