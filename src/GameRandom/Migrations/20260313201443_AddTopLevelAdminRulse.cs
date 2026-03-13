using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameRandom.Migrations
{
    /// <inheritdoc />
    public partial class AddTopLevelAdminRulse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTopAdmin",
                table: "Admins",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTopAdmin",
                table: "Admins");
        }
    }
}
