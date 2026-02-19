using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameProgresses",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppID = table.Column<int>(type: "integer", nullable: false),
                    PlayerID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    AppName = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    BeginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameProgresses", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Lobbies",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LobbyID = table.Column<long>(type: "bigint", nullable: false),
                    MembersCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lobbies", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SteamID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LobbyID = table.Column<long>(type: "bigint", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    AvatarURL = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserGames",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsHaveGame = table.Column<bool>(type: "boolean", nullable: false),
                    GameID = table.Column<int>(type: "integer", nullable: false),
                    AppName = table.Column<string>(type: "text", nullable: false),
                    LeftDays = table.Column<int>(type: "integer", nullable: false),
                    BeginData = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndData = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGames", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserGames_GameProgresses_GameID",
                        column: x => x.GameID,
                        principalTable: "GameProgresses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_GameID",
                table: "UserGames",
                column: "GameID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lobbies");

            migrationBuilder.DropTable(
                name: "UserGames");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "GameProgresses");
        }
    }
}
