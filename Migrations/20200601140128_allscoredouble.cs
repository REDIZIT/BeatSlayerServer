using Microsoft.EntityFrameworkCore.Migrations;

namespace BeatSlayerServer.Migrations
{
    public partial class allscoredouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "AllScore",
                table: "Players",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "AllScore",
                table: "Players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double));
        }
    }
}
