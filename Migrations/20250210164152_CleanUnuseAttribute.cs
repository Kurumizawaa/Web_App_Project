using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_app_project.Migrations
{
    /// <inheritdoc />
    public partial class CleanUnuseAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AnnouncementRecipients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Enrollments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AnnouncementRecipients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
