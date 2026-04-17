using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiBaBoulder.Migrations
{
    /// <inheritdoc />
    public partial class RenamedUserToCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpraywallProblems_Users_UserId",
                table: "SpraywallProblems");

            migrationBuilder.DropIndex(
                name: "IX_SpraywallProblems_UserId",
                table: "SpraywallProblems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SpraywallProblems");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "SpraywallProblems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SpraywallProblems_CreatorId",
                table: "SpraywallProblems",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpraywallProblems_Users_CreatorId",
                table: "SpraywallProblems",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpraywallProblems_Users_CreatorId",
                table: "SpraywallProblems");

            migrationBuilder.DropIndex(
                name: "IX_SpraywallProblems_CreatorId",
                table: "SpraywallProblems");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "SpraywallProblems");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "SpraywallProblems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpraywallProblems_UserId",
                table: "SpraywallProblems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpraywallProblems_Users_UserId",
                table: "SpraywallProblems",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
