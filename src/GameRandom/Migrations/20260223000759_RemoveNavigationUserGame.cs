using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNavigationUserGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGames_GameProgresses_AppId",
                table: "UserGames");

            migrationBuilder.DropIndex(
                name: "IX_UserGames_AppId",
                table: "UserGames");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_GameProgresses_AppID",
                table: "GameProgresses");

            migrationBuilder.AlterColumn<int>(
                name: "AppId",
                table: "UserGames",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AppId",
                table: "UserGames",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_GameProgresses_AppID",
                table: "GameProgresses",
                column: "AppID");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_AppId",
                table: "UserGames",
                column: "AppId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGames_GameProgresses_AppId",
                table: "UserGames",
                column: "AppId",
                principalTable: "GameProgresses",
                principalColumn: "AppID");
        }
    }
}
