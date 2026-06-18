using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoxFox.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewerToCourseDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewStartedAt",
                table: "CourseDrafts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewerId",
                table: "CourseDrafts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseDrafts_ReviewerId",
                table: "CourseDrafts",
                column: "ReviewerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseDrafts_Users_ReviewerId",
                table: "CourseDrafts",
                column: "ReviewerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseDrafts_Users_ReviewerId",
                table: "CourseDrafts");

            migrationBuilder.DropIndex(
                name: "IX_CourseDrafts_ReviewerId",
                table: "CourseDrafts");

            migrationBuilder.DropColumn(
                name: "ReviewStartedAt",
                table: "CourseDrafts");

            migrationBuilder.DropColumn(
                name: "ReviewerId",
                table: "CourseDrafts");
        }
    }
}
