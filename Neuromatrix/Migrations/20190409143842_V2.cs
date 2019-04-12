using Microsoft.EntityFrameworkCore.Migrations;

namespace Neuromatrix.Migrations
{
    public partial class V2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableNotification",
                table: "Guilds",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeMessage",
                table: "Guilds",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableNotification",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "WelcomeMessage",
                table: "Guilds");
        }
    }
}
