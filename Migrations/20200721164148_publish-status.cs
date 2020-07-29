using Microsoft.EntityFrameworkCore.Migrations;

namespace BeatSlayerServer.Migrations
{
    public partial class publishstatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublishStatus",
                table: "MapInfo",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "MapInfo");
        }
    }
}
