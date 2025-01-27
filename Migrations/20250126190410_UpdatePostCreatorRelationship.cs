using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_app_project.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostCreatorRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Accounts_CreatorId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreatorId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Posts");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreaterId",
                table: "Posts",
                column: "CreaterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Accounts_CreaterId",
                table: "Posts",
                column: "CreaterId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Accounts_CreaterId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreaterId",
                table: "Posts");

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Posts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatorId",
                table: "Posts",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Accounts_CreatorId",
                table: "Posts",
                column: "CreatorId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }
    }
}
