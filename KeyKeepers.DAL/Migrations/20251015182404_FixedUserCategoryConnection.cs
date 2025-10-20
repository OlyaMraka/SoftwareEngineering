using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyKeepers.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixedUserCategoryConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrivateCategories_CommunityUsers_CommunityUserId",
                table: "PrivateCategories");

            migrationBuilder.RenameColumn(
                name: "CommunityUserId",
                table: "PrivateCategories",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PrivateCategories_CommunityUserId",
                table: "PrivateCategories",
                newName: "IX_PrivateCategories_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateCategories_Users_UserId",
                table: "PrivateCategories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrivateCategories_Users_UserId",
                table: "PrivateCategories");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PrivateCategories",
                newName: "CommunityUserId");

            migrationBuilder.RenameIndex(
                name: "IX_PrivateCategories_UserId",
                table: "PrivateCategories",
                newName: "IX_PrivateCategories_CommunityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateCategories_CommunityUsers_CommunityUserId",
                table: "PrivateCategories",
                column: "CommunityUserId",
                principalTable: "CommunityUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
