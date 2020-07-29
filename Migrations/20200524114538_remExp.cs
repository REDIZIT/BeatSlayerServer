using Microsoft.EntityFrameworkCore.Migrations;

namespace BeatSlayerServer.Migrations
{
    public partial class remExp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exp",
                table: "Players");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Exp",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
