using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFinishedGameTableStruct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EndGames_GameProgresses_GameProgressesId",
                table: "EndGames");

            migrationBuilder.DropIndex(
                name: "IX_EndGames_GameProgressesId",
                table: "EndGames");

            migrationBuilder.DropColumn(
                name: "GameProgressesId",
                table: "EndGames");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "EndGames");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EndGames");

            migrationBuilder.RenameColumn(
                name: "AppId",
                table: "EndGames",
                newName: "GameProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_EndGames_GameProgressId",
                table: "EndGames",
                column: "GameProgressId");

            migrationBuilder.AddForeignKey(
                name: "FK_EndGames_GameProgresses_GameProgressId",
                table: "EndGames",
                column: "GameProgressId",
                principalTable: "GameProgresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EndGames_GameProgresses_GameProgressId",
                table: "EndGames");

            migrationBuilder.DropIndex(
                name: "IX_EndGames_GameProgressId",
                table: "EndGames");

            migrationBuilder.RenameColumn(
                name: "GameProgressId",
                table: "EndGames",
                newName: "AppId");

            migrationBuilder.AddColumn<int>(
                name: "GameProgressesId",
                table: "EndGames",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "EndGames",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UserId",
                table: "EndGames",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_EndGames_GameProgressesId",
                table: "EndGames",
                column: "GameProgressesId");

            migrationBuilder.AddForeignKey(
                name: "FK_EndGames_GameProgresses_GameProgressesId",
                table: "EndGames",
                column: "GameProgressesId",
                principalTable: "GameProgresses",
                principalColumn: "Id");
        }
    }
}
