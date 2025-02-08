using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_app_project.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementRecipient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountAnnouncementRecipient");

            migrationBuilder.CreateTable(
                name: "AnnouncementRecipients",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementRecipients", x => new { x.AccountId, x.AnnouncementId });
                    table.ForeignKey(
                        name: "FK_AnnouncementRecipients_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnnouncementRecipients_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRecipients_AnnouncementId",
                table: "AnnouncementRecipients",
                column: "AnnouncementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnouncementRecipients");

            migrationBuilder.CreateTable(
                name: "AccountAnnouncementRecipient",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnnouncementId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAnnouncementRecipient", x => new { x.AccountId, x.AnnouncementId });
                    table.ForeignKey(
                        name: "FK_AccountAnnouncementRecipient_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountAnnouncementRecipient_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAnnouncementRecipient_AnnouncementId",
                table: "AccountAnnouncementRecipient",
                column: "AnnouncementId");
        }
    }
}
