using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiBaBoulder.Migrations
{
    /// <inheritdoc />
    public partial class AddedAccessRestrictionAndMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUri",
                table: "Spraywalls",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Coordinates",
                table: "Sectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportantInfo",
                table: "Sectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Sectors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUri",
                table: "Sectors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Coordinates",
                table: "Blocs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OutdoorAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportantInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviewImageUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutdoorAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SectorImages",
                columns: table => new
                {
                    Uri = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorImages", x => new { x.SectorId, x.Uri, x.ResourceType });
                    table.ForeignKey(
                        name: "FK_SectorImages_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSectorAccesses",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessSourceType = table.Column<int>(type: "int", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSectorAccesses", x => new { x.UserId, x.SectorId });
                    table.ForeignKey(
                        name: "FK_UserSectorAccesses_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSectorAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutdoorAreaImages",
                columns: table => new
                {
                    Uri = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    OutdoorAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutdoorAreaImages", x => new { x.OutdoorAreaId, x.Uri, x.ResourceType });
                    table.ForeignKey(
                        name: "FK_OutdoorAreaImages_OutdoorAreas_OutdoorAreaId",
                        column: x => x.OutdoorAreaId,
                        principalTable: "OutdoorAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutdoorAreaSector",
                columns: table => new
                {
                    OutdoorAreasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutdoorAreaSector", x => new { x.OutdoorAreasId, x.SectorsId });
                    table.ForeignKey(
                        name: "FK_OutdoorAreaSector_OutdoorAreas_OutdoorAreasId",
                        column: x => x.OutdoorAreasId,
                        principalTable: "OutdoorAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutdoorAreaSector_Sectors_SectorsId",
                        column: x => x.SectorsId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutdoorAreaSector_SectorsId",
                table: "OutdoorAreaSector",
                column: "SectorsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSectorAccesses_SectorId",
                table: "UserSectorAccesses",
                column: "SectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutdoorAreaImages");

            migrationBuilder.DropTable(
                name: "OutdoorAreaSector");

            migrationBuilder.DropTable(
                name: "SectorImages");

            migrationBuilder.DropTable(
                name: "UserSectorAccesses");

            migrationBuilder.DropTable(
                name: "OutdoorAreas");

            migrationBuilder.DropColumn(
                name: "PreviewImageUri",
                table: "Spraywalls");

            migrationBuilder.DropColumn(
                name: "Coordinates",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "ImportantInfo",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "PreviewImageUri",
                table: "Sectors");

            migrationBuilder.DropColumn(
                name: "Coordinates",
                table: "Blocs");
        }
    }
}
