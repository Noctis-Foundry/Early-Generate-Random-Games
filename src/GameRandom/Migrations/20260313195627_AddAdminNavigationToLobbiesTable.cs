using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminNavigationToLobbiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LobbiesLobbyId",
                table: "Admins",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_LobbiesLobbyId",
                table: "Admins",
                column: "LobbiesLobbyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Lobbies_LobbiesLobbyId",
                table: "Admins",
                column: "LobbiesLobbyId",
                principalTable: "Lobbies",
                principalColumn: "LobbyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Lobbies_LobbiesLobbyId",
                table: "Admins");

            migrationBuilder.DropIndex(
                name: "IX_Admins_LobbiesLobbyId",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "LobbiesLobbyId",
                table: "Admins");
        }
    }
}
