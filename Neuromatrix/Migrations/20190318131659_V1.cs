using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class V1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    ID = table.Column<ulong>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    OwnerId = table.Column<ulong>(nullable: false),
                    NotificationChannel = table.Column<ulong>(nullable: false, defaultValue: 0ul),
                    LoggingChannel = table.Column<ulong>(nullable: false, defaultValue: 0ul),
                    EnableLogging = table.Column<bool>(nullable: false, defaultValue: false)
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
