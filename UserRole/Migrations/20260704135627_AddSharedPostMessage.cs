using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserRole.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedPostMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PostId",
                table: "Messages",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Posts_PostId",
                table: "Messages",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Posts_PostId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_PostId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "Messages");
        }
    }
}
