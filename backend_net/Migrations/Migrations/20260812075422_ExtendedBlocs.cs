using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiBaBoulder.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedBlocs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocs_Sectors_SectorId",
                table: "Blocs");

            migrationBuilder.AlterColumn<Guid>(
                name: "SectorId",
                table: "Blocs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "BlocId",
                table: "Blocs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocs_BlocId",
                table: "Blocs",
                column: "BlocId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocs_Blocs_BlocId",
                table: "Blocs",
                column: "BlocId",
                principalTable: "Blocs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocs_Sectors_SectorId",
                table: "Blocs",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocs_Blocs_BlocId",
                table: "Blocs");

            migrationBuilder.DropForeignKey(
                name: "FK_Blocs_Sectors_SectorId",
                table: "Blocs");

            migrationBuilder.DropIndex(
                name: "IX_Blocs_BlocId",
                table: "Blocs");

            migrationBuilder.DropColumn(
                name: "BlocId",
                table: "Blocs");

            migrationBuilder.AlterColumn<Guid>(
                name: "SectorId",
                table: "Blocs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Blocs_Sectors_SectorId",
                table: "Blocs",
                column: "SectorId",
                principalTable: "Sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
