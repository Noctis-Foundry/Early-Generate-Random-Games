using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class UserGameTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGames_GameProgresses_GameID",
                table: "UserGames");

            migrationBuilder.DropIndex(
                name: "IX_UserGames_GameID",
                table: "UserGames");

            migrationBuilder.DropColumn(
                name: "AppName",
                table: "UserGames");

            migrationBuilder.DropColumn(
                name: "BeginData",
                table: "UserGames");

            migrationBuilder.DropColumn(
                name: "EndData",
                table: "UserGames");

            migrationBuilder.DropColumn(
                name: "GameID",
                table: "UserGames");

            migrationBuilder.DropColumn(
                name: "IsHaveGame",
                table: "UserGames");

            migrationBuilder.RenameColumn(
                name: "LeftDays",
                table: "UserGames",
                newName: "AppId");

            migrationBuilder.AddColumn<decimal>(
                name: "UserId",
                table: "UserGames",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_GameProgresses_AppID",
                table: "GameProgresses",
                column: "AppID");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_AppId",
                table: "UserGames",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_UserId",
                table: "UserGames",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGames_GameProgresses_AppId",
                table: "UserGames",
                column: "AppId",
                principalTable: "GameProgresses",
                principalColumn: "AppID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGames_GameProgresses_AppId",
                table: "UserGames");

            migrationBuilder.DropIndex(
                name: "IX_UserGames_AppId",
                table: "UserGames");

            migrationBuilder.DropIndex(
                name: "IX_UserGames_UserId",
                table: "UserGames");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_GameProgresses_AppID",
                table: "GameProgresses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserGames");

            migrationBuilder.RenameColumn(
                name: "AppId",
                table: "UserGames",
                newName: "LeftDays");

            migrationBuilder.AddColumn<string>(
                name: "AppName",
                table: "UserGames",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "BeginData",
                table: "UserGames",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndData",
                table: "UserGames",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GameID",
                table: "UserGames",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHaveGame",
                table: "UserGames",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_GameID",
                table: "UserGames",
                column: "GameID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGames_GameProgresses_GameID",
                table: "UserGames",
                column: "GameID",
                principalTable: "GameProgresses",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
