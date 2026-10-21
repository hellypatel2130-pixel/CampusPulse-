using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserRole.Migrations
{
    /// <inheritdoc />
    public partial class FixRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UsersId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_AspNetUsers_UsersId",
                table: "Likes");

      

            migrationBuilder.DropIndex(
                name: "IX_Likes_UsersId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UsersId",
                table: "Comments");

          

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsersId",
                table: "Posts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsersId",
                table: "Likes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsersId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UsersId",
                table: "Posts",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UsersId",
                table: "Likes",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UsersId",
                table: "Comments",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UsersId",
                table: "Comments",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_AspNetUsers_UsersId",
                table: "Likes",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");


            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_UsersId",
                table: "Posts",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
