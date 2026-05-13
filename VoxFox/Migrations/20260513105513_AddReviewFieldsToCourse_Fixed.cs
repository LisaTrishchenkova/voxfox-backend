using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoxFox.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewFieldsToCourse_Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<int>(
            //     name: "ReviewCount",
            //     table: "Courses",
            //     type: "integer",
            //     nullable: false,
            //     defaultValue: 0); // тут давай если есть такая колонка в жэторй таблице то пропустим создание
            //
            // migrationBuilder.AddColumn<DateTime>(
            //     name: "ReviewStartedAt",
            //     table: "Courses",
            //     type: "timestamp with time zone",
            //     nullable: true);
            //
            // migrationBuilder.AddColumn<Guid>(
            //     name: "ReviewerId",
            //     table: "Courses",
            //     type: "uuid",
            //     nullable: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "IX_Courses_ReviewerId",
            //     table: "Courses",
            //     column: "ReviewerId");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_Courses_Users_ReviewerId",
            //     table: "Courses",
            //     column: "ReviewerId",
            //     principalTable: "Users",
            //     principalColumn: "Id",
            //     onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Courses_Users_ReviewerId",
            //     table: "Courses");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_Courses_ReviewerId",
            //     table: "Courses");
            //
            // migrationBuilder.DropColumn(
            //     name: "ReviewCount",
            //     table: "Courses");// тут давай если есть такая колонка в жэторй таблице нет, то пропустим удаление
            //
            // migrationBuilder.DropColumn(
            //     name: "ReviewStartedAt",
            //     table: "Courses");
            //
            // migrationBuilder.DropColumn(
            //     name: "ReviewerId",
            //     table: "Courses");
        }
    }
}
