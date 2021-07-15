using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fuselage.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Catalysts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Locale = table.Column<string>(maxLength: 5, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    Icon = table.Column<string>(maxLength: 2048, nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    DropLocation = table.Column<string>(maxLength: 200, nullable: true),
                    Objectives = table.Column<string>(maxLength: 200, nullable: true),
                    Masterwork = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalysts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Icon = table.Column<string>(maxLength: 2048, nullable: true),
                    MaxSpace = table.Column<byte>(nullable: false),
                    Type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Welcomes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Locale = table.Column<string>(maxLength: 5, nullable: true),
                    Message = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Welcomes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MilestoneLocales",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MilestoneId = table.Column<int>(nullable: false),
                    Locale = table.Column<string>(maxLength: 5, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    Type = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilestoneLocales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilestoneLocales_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onUpdate: ReferentialAction.Cascade,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneLocales_MilestoneId",
                table: "MilestoneLocales",
                column: "MilestoneId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Catalysts");

            migrationBuilder.DropTable(
                name: "MilestoneLocales");

            migrationBuilder.DropTable(
                name: "Welcomes");

            migrationBuilder.DropTable(
                name: "Milestones");
        }
    }
}
