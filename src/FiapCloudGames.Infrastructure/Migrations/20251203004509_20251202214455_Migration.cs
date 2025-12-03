using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiapCloudGames.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20251202214455_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId",
                table: "Libraries");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId_GameId_PurchaseId",
                table: "Libraries",
                columns: new[] { "UserId", "GameId", "PurchaseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_UserId_GameId_PurchaseId",
                table: "Libraries");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UserId",
                table: "Libraries",
                column: "UserId");
        }
    }
}
