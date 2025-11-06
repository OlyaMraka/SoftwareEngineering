using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyKeepers.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommunityUsers_UserName",
                table: "CommunityUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "CommunityUsers");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "CommunityUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "CommunityUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "CommunityUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityUsers_UserName",
                table: "CommunityUsers",
                column: "UserName",
                unique: true);
        }
    }
}
