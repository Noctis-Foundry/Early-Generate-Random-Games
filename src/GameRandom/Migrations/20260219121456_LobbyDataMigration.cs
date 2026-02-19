using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class LobbyDataMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LobbyID",
                table: "Lobbies",
                newName: "LobbyId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Lobbies_LobbyId",
                table: "Lobbies",
                column: "LobbyId");

            migrationBuilder.CreateTable(
                name: "LobbyData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LobbyId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LobbyData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LobbyData_Lobbies_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "Lobbies",
                        principalColumn: "LobbyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LobbyData_LobbyId",
                table: "LobbyData",
                column: "LobbyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LobbyData");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Lobbies_LobbyId",
                table: "Lobbies");

            migrationBuilder.RenameColumn(
                name: "LobbyId",
                table: "Lobbies",
                newName: "LobbyID");
        }
    }
}
