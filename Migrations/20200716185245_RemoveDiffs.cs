using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace BeatSlayerServer.Migrations
{
    public partial class RemoveDiffs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DifficultyInfo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DifficultyInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CubesCount = table.Column<int>(type: "int", nullable: false),
                    Dislikes = table.Column<int>(type: "int", nullable: false),
                    Downloads = table.Column<int>(type: "int", nullable: false),
                    InnerId = table.Column<int>(type: "int", nullable: false),
                    Likes = table.Column<int>(type: "int", nullable: false),
                    LinesCount = table.Column<int>(type: "int", nullable: false),
                    MapId = table.Column<int>(type: "int", nullable: true),
                    MaxRP = table.Column<float>(type: "float", nullable: false),
                    MaxScore = table.Column<float>(type: "float", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PlayCount = table.Column<int>(type: "int", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DifficultyInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DifficultyInfo_MapInfo_MapId",
                        column: x => x.MapId,
                        principalTable: "MapInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DifficultyInfo_MapId",
                table: "DifficultyInfo",
                column: "MapId");
        }
    }
}
