using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoxFox.Migrations
{
    /// <inheritdoc />
    public partial class FixSetDefaultStatusForCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Courses""
                SET ""Status"" =
                    CASE
                        WHEN ""IsPublished"" = true THEN 'Published'
                        ELSE 'Draft'
                    END
                WHERE ""Status"" = 'Draft' OR ""Status"" IS NULL
            ");

            migrationBuilder.DropColumn("IsPublished", "Courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>("IsPublished", "Courses", nullable: false, defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE ""Courses""
                SET ""IsPublished"" = (""Status"" = 'Published')
            ");
        }
    }
}
