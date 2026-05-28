using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoxFox.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FullDescription = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    Level = table.Column<string>(type: "text", nullable: false, defaultValue: "Beginner"),
                    CertificateEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseDrafts_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseDrafts_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DraftSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalSectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftSections_CourseDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "CourseDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftTags_CourseDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "CourseDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftLessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DraftSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalLessonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftLessons_DraftSections_DraftSectionId",
                        column: x => x.DraftSectionId,
                        principalTable: "DraftSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DraftLessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectIndex = table.Column<int>(type: "integer", nullable: true),
                    CorrectIndexes = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftTasks_DraftLessons_DraftLessonId",
                        column: x => x.DraftLessonId,
                        principalTable: "DraftLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseDrafts_AuthorId",
                table: "CourseDrafts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseDrafts_CourseId",
                table: "CourseDrafts",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftLessons_DraftSectionId",
                table: "DraftLessons",
                column: "DraftSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftSections_DraftId",
                table: "DraftSections",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftTags_DraftId",
                table: "DraftTags",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftTasks_DraftLessonId",
                table: "DraftTasks",
                column: "DraftLessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftTags");

            migrationBuilder.DropTable(
                name: "DraftTasks");

            migrationBuilder.DropTable(
                name: "DraftLessons");

            migrationBuilder.DropTable(
                name: "DraftSections");

            migrationBuilder.DropTable(
                name: "CourseDrafts");
        }
    }
}
