using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiBaBoulder.Migrations
{
    /// <inheritdoc />
    public partial class AddedPreviewImageForBloc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUri",
                table: "Blocs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewImageUri",
                table: "Blocs");
        }
    }
}
