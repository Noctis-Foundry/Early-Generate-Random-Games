using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class RefactorClassesId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SteamID",
                table: "Users",
                newName: "SteamId");

            migrationBuilder.RenameColumn(
                name: "LobbyID",
                table: "Users",
                newName: "LobbyId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "UserGames",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Lobbies",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PlayerID",
                table: "GameProgresses",
                newName: "PlayerId");

            migrationBuilder.RenameColumn(
                name: "AppID",
                table: "GameProgresses",
                newName: "AppId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "GameProgresses",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SteamId",
                table: "Users",
                newName: "SteamID");

            migrationBuilder.RenameColumn(
                name: "LobbyId",
                table: "Users",
                newName: "LobbyID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserGames",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Lobbies",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "GameProgresses",
                newName: "PlayerID");

            migrationBuilder.RenameColumn(
                name: "AppId",
                table: "GameProgresses",
                newName: "AppID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "GameProgresses",
                newName: "ID");
        }
    }
}
