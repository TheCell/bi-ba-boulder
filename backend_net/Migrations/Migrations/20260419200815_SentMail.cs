using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiBaBoulder.Migrations
{
    /// <inheritdoc />
    public partial class SentMail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    To = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Cc = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Bcc = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mails");
        }
    }
}
