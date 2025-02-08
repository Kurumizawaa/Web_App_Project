using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_app_project.Migrations
{
    /// <inheritdoc />
    public partial class ChangePostStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsOpen",
                table: "Posts",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Posts",
                newName: "IsOpen");
        }
    }
}
