using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNamesAndLobbiesMemberCountType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LobbyContexts",
                table: "LobbyContexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameTables",
                table: "GameTables");

            migrationBuilder.RenameTable(
                name: "LobbyContexts",
                newName: "LobbyUserContexts");

            migrationBuilder.RenameTable(
                name: "GameTables",
                newName: "GameProgress");

            migrationBuilder.AlterColumn<int>(
                name: "MemberCount",
                table: "Lobbies",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LobbyUserContexts",
                table: "LobbyUserContexts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameProgress",
                table: "GameProgress",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LobbyUserContexts",
                table: "LobbyUserContexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameProgress",
                table: "GameProgress");

            migrationBuilder.RenameTable(
                name: "LobbyUserContexts",
                newName: "LobbyContexts");

            migrationBuilder.RenameTable(
                name: "GameProgress",
                newName: "GameTables");

            migrationBuilder.AlterColumn<long>(
                name: "MemberCount",
                table: "Lobbies",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LobbyContexts",
                table: "LobbyContexts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameTables",
                table: "GameTables",
                column: "Id");
        }
    }
}
