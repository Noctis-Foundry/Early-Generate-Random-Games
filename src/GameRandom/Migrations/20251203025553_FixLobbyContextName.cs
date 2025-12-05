using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class FixLobbyContextName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LobbyUserContexts",
                table: "LobbyUserContexts");

            migrationBuilder.RenameTable(
                name: "LobbyUserContexts",
                newName: "LobbyUserContext");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LobbyUserContext",
                table: "LobbyUserContext",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LobbyUserContext",
                table: "LobbyUserContext");

            migrationBuilder.RenameTable(
                name: "LobbyUserContext",
                newName: "LobbyUserContexts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LobbyUserContexts",
                table: "LobbyUserContexts",
                column: "Id");
        }
    }
}
