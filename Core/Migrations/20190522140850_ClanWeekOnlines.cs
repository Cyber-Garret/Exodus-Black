using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class ClanWeekOnlines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClanWeekOnlines",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClanId = table.Column<long>(nullable: false),
                    MembershipType = table.Column<long>(nullable: false),
                    MembershipId = table.Column<string>(nullable: true),
                    IconPath = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    BungieName = table.Column<string>(nullable: true),
                    ClanJoinDate = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanWeekOnlines", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanWeekOnlines");
        }
    }
}
