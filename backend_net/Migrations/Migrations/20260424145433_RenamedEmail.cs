using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiBaBoulder.Migrations
{
    /// <inheritdoc />
    public partial class RenamedEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Mails",
                table: "Mails");

            migrationBuilder.RenameTable(
                name: "Mails",
                newName: "Emails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emails",
                table: "Emails",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Emails",
                table: "Emails");

            migrationBuilder.RenameTable(
                name: "Emails",
                newName: "Mails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mails",
                table: "Mails",
                column: "Id");
        }
    }
}
