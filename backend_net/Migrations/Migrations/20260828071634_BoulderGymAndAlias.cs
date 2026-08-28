using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiBaBoulder.Migrations
{
    /// <inheritdoc />
    public partial class BoulderGymAndAlias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BoulderGymId",
                table: "Spraywalls",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Spraywalls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BoulderGyms",
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
                    table.PrimaryKey("PK_BoulderGyms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoulderGymImages",
                columns: table => new
                {
                    Uri = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ResourceType = table.Column<int>(type: "int", nullable: false),
                    BoulderGymId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoulderGymImages", x => new { x.BoulderGymId, x.Uri, x.ResourceType });
                    table.ForeignKey(
                        name: "FK_BoulderGymImages_BoulderGyms_BoulderGymId",
                        column: x => x.BoulderGymId,
                        principalTable: "BoulderGyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UriAliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BoulderGymId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutdoorAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_UriAliases", x => x.Id);
                    table.CheckConstraint("CK_UriAlias_OnlyOneForeignKey", "([BoulderGymId] IS NOT NULL AND [OutdoorAreaId] IS NULL) OR ([BoulderGymId] IS NULL AND [OutdoorAreaId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_UriAliases_BoulderGyms_BoulderGymId",
                        column: x => x.BoulderGymId,
                        principalTable: "BoulderGyms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UriAliases_OutdoorAreas_OutdoorAreaId",
                        column: x => x.OutdoorAreaId,
                        principalTable: "OutdoorAreas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Spraywalls_BoulderGymId",
                table: "Spraywalls",
                column: "BoulderGymId");

            migrationBuilder.CreateIndex(
                name: "IX_UriAliases_BoulderGymId",
                table: "UriAliases",
                column: "BoulderGymId");

            migrationBuilder.CreateIndex(
                name: "IX_UriAliases_OutdoorAreaId",
                table: "UriAliases",
                column: "OutdoorAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Spraywalls_BoulderGyms_BoulderGymId",
                table: "Spraywalls",
                column: "BoulderGymId",
                principalTable: "BoulderGyms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Spraywalls_BoulderGyms_BoulderGymId",
                table: "Spraywalls");

            migrationBuilder.DropTable(
                name: "BoulderGymImages");

            migrationBuilder.DropTable(
                name: "UriAliases");

            migrationBuilder.DropTable(
                name: "BoulderGyms");

            migrationBuilder.DropIndex(
                name: "IX_Spraywalls_BoulderGymId",
                table: "Spraywalls");

            migrationBuilder.DropColumn(
                name: "BoulderGymId",
                table: "Spraywalls");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Spraywalls");
        }
    }
}
