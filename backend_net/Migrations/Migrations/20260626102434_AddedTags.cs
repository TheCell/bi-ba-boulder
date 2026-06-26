using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiBaBoulder.Migrations
{
    /// <inheritdoc />
    public partial class AddedTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FreeFeet",
                table: "SpraywallProblems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCircuit",
                table: "SpraywallProblems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NoMatch",
                table: "SpraywallProblems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeFeet",
                table: "SpraywallProblems");

            migrationBuilder.DropColumn(
                name: "IsCircuit",
                table: "SpraywallProblems");

            migrationBuilder.DropColumn(
                name: "NoMatch",
                table: "SpraywallProblems");
        }
    }
}
