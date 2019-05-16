using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class InitialCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gears",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    IconUrl = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    PerkName = table.Column<string>(nullable: true),
                    PerkDescription = table.Column<string>(nullable: true),
                    SecondPerkName = table.Column<string>(nullable: true),
                    SecondPerkDescription = table.Column<string>(nullable: true),
                    DropLocation = table.Column<string>(nullable: true),
                    Catalyst = table.Column<int>(nullable: false),
                    WhereCatalystDrop = table.Column<string>(nullable: true),
                    CatalystQuest = table.Column<string>(nullable: true),
                    CatalystBonus = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    ID = table.Column<ulong>(nullable: false),
                    NotificationChannel = table.Column<ulong>(nullable: false, defaultValue: 0ul),
                    LoggingChannel = table.Column<ulong>(nullable: false, defaultValue: 0ul),
                    EnableLogging = table.Column<bool>(nullable: false, defaultValue: false),
                    EnableNotification = table.Column<bool>(nullable: false, defaultValue: false),
                    WelcomeMessage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gears");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
