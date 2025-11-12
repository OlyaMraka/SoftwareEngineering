using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyKeepers.DAL.Migrations
{
    /// <inheritdoc />
    public partial class HotFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JoinRequest_CommunityUsers_SenderId",
                table: "JoinRequest");

            migrationBuilder.AddForeignKey(
                name: "FK_JoinRequest_CommunityUsers_SenderId",
                table: "JoinRequest",
                column: "SenderId",
                principalTable: "CommunityUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JoinRequest_CommunityUsers_SenderId",
                table: "JoinRequest");

            migrationBuilder.AddForeignKey(
                name: "FK_JoinRequest_CommunityUsers_SenderId",
                table: "JoinRequest",
                column: "SenderId",
                principalTable: "CommunityUsers",
                principalColumn: "Id");
        }
    }
}
