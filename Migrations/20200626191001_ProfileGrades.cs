using Microsoft.EntityFrameworkCore.Migrations;

namespace BeatSlayerServer.Migrations
{
    public partial class ProfileGrades : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "A",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "B",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "C",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "D",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "S",
                table: "Players",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SS",
                table: "Players",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "A",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "B",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "C",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "D",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "S",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "SS",
                table: "Players");
        }
    }
}
