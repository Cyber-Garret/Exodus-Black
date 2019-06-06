using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class V1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Destiny2Clans",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(nullable: false),
                    Motto = table.Column<string>(nullable: true),
                    About = table.Column<string>(nullable: true),
                    MemberCount = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destiny2Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gears",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
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
                    Catalyst = table.Column<bool>(nullable: false),
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
                    ID = table.Column<decimal>(nullable: false),
                    NotificationChannel = table.Column<decimal>(nullable: false, defaultValue: 0m),
                    LoggingChannel = table.Column<decimal>(nullable: false, defaultValue: 0m),
                    EnableLogging = table.Column<bool>(nullable: false, defaultValue: false),
                    EnableNotification = table.Column<bool>(nullable: false, defaultValue: false),
                    WelcomeMessage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Destiny2Clan_Members",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    DestinyMembershipType = table.Column<long>(nullable: false),
                    DestinyMembershipId = table.Column<string>(nullable: true),
                    BungieMembershipType = table.Column<long>(nullable: true),
                    BungieMembershipId = table.Column<string>(nullable: true),
                    IconPath = table.Column<string>(nullable: true),
                    ClanJoinDate = table.Column<DateTimeOffset>(nullable: true),
                    Destiny2ClanId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destiny2Clan_Members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Destiny2Clan_Members_Destiny2Clans_Destiny2ClanId",
                        column: x => x.Destiny2ClanId,
                        principalTable: "Destiny2Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Destiny2Clan_Members_Destiny2ClanId",
                table: "Destiny2Clan_Members",
                column: "Destiny2ClanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Destiny2Clan_Members");

            migrationBuilder.DropTable(
                name: "Gears");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Destiny2Clans");
        }
    }
}
