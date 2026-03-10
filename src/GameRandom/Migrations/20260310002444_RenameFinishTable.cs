using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class RenameFinishTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EndGames_GameProgresses_GameProgressId",
                table: "EndGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EndGames",
                table: "EndGames");

            migrationBuilder.RenameTable(
                name: "EndGames",
                newName: "FinishedGames");

            migrationBuilder.RenameIndex(
                name: "IX_EndGames_GameProgressId",
                table: "FinishedGames",
                newName: "IX_FinishedGames_GameProgressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinishedGames",
                table: "FinishedGames",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinishedGames_GameProgresses_GameProgressId",
                table: "FinishedGames",
                column: "GameProgressId",
                principalTable: "GameProgresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinishedGames_GameProgresses_GameProgressId",
                table: "FinishedGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinishedGames",
                table: "FinishedGames");

            migrationBuilder.RenameTable(
                name: "FinishedGames",
                newName: "EndGames");

            migrationBuilder.RenameIndex(
                name: "IX_FinishedGames_GameProgressId",
                table: "EndGames",
                newName: "IX_EndGames_GameProgressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EndGames",
                table: "EndGames",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EndGames_GameProgresses_GameProgressId",
                table: "EndGames",
                column: "GameProgressId",
                principalTable: "GameProgresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
