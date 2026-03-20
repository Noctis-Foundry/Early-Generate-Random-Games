using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class SelecteUniqueParametrsForFinishedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinishedGames_GameProgressId",
                table: "FinishedGames");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGames_GameProgressId",
                table: "FinishedGames",
                column: "GameProgressId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinishedGames_GameProgressId",
                table: "FinishedGames");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGames_GameProgressId",
                table: "FinishedGames",
                column: "GameProgressId");
        }
    }
}
