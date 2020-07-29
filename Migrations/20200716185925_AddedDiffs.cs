using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace BeatSlayerServer.Migrations
{
    public partial class AddedDiffs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DifficultyInfoId",
                table: "Replays",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DifficultyInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MapId = table.Column<int>(nullable: true),
                    InnerId = table.Column<int>(nullable: false),
                    Stars = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Downloads = table.Column<int>(nullable: false),
                    PlayCount = table.Column<int>(nullable: false),
                    Likes = table.Column<int>(nullable: false),
                    Dislikes = table.Column<int>(nullable: false),
                    CubesCount = table.Column<int>(nullable: false),
                    LinesCount = table.Column<int>(nullable: false),
                    BombsCount = table.Column<int>(nullable: false),
                    MaxScore = table.Column<float>(nullable: false),
                    MaxRP = table.Column<float>(nullable: false)
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
                name: "IX_Replays_DifficultyInfoId",
                table: "Replays",
                column: "DifficultyInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_DifficultyInfo_MapId",
                table: "DifficultyInfo",
                column: "MapId");

            migrationBuilder.AddForeignKey(
                name: "FK_Replays_DifficultyInfo_DifficultyInfoId",
                table: "Replays",
                column: "DifficultyInfoId",
                principalTable: "DifficultyInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Replays_DifficultyInfo_DifficultyInfoId",
                table: "Replays");

            migrationBuilder.DropTable(
                name: "DifficultyInfo");

            migrationBuilder.DropIndex(
                name: "IX_Replays_DifficultyInfoId",
                table: "Replays");

            migrationBuilder.DropColumn(
                name: "DifficultyInfoId",
                table: "Replays");
        }
    }
}
