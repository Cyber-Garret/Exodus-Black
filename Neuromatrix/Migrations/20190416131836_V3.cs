using Microsoft.EntityFrameworkCore.Migrations;

namespace Neuromatrix.Migrations
{
    public partial class V3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Guilds");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Guilds",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "OwnerId",
                table: "Guilds",
                nullable: false,
                defaultValue: 0ul);
        }
    }
}
