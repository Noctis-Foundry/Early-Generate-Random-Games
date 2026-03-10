using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class FinishedImproveProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsImprove",
                table: "EndGames",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImprove",
                table: "EndGames");
        }
    }
}
