using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class V3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Catalyst_Categories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalyst_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Catalysts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(nullable: false),
                    WeaponName = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    CatalystLocation = table.Column<string>(nullable: true),
                    CatalystQuest = table.Column<string>(nullable: true),
                    CatalystBonus = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalysts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Catalysts_Catalyst_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Catalyst_Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Catalysts_CategoryId",
                table: "Catalysts",
                column: "CategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "Catalysts");

            migrationBuilder.DropTable(
                name: "Catalyst_Categories");
        }
    }
}
