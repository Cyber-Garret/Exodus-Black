using Microsoft.EntityFrameworkCore.Migrations;

namespace Neuromatrix.Migrations
{
    public partial class Administration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuildID = table.Column<ulong>(nullable: false),
                    GuildName = table.Column<string>(nullable: true),
                    GuildOwnerId = table.Column<ulong>(nullable: false),
                    NotificationChannel = table.Column<ulong>(nullable: false),
                    EnableLogging = table.Column<bool>(nullable: false),
                    LoggingChannel = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
