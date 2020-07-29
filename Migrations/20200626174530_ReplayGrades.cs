using Microsoft.EntityFrameworkCore.Migrations;

namespace BeatSlayerServer.Migrations
{
    public partial class ReplayGrades : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Replays",
                nullable: false,
                defaultValue: 6);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Replays");
        }
    }
}
