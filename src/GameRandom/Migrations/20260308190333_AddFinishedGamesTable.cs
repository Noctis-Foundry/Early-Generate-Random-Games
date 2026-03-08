using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class AddFinishedGamesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EndGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    AppId = table.Column<int>(type: "integer", nullable: false),
                    ScreenShot = table.Column<byte[]>(type: "bytea", nullable: true),
                    Nickname = table.Column<string>(type: "text", nullable: true),
                    GameProgressesId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndGames_GameProgresses_GameProgressesId",
                        column: x => x.GameProgressesId,
                        principalTable: "GameProgresses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EndGames_GameProgressesId",
                table: "EndGames",
                column: "GameProgressesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EndGames");
        }
    }
}
