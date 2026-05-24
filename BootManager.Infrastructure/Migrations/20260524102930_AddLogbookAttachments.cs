using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogbookAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogbookAttachmentsDirectory",
                table: "OperationalSettings",
                type: "TEXT",
                maxLength: 1024,
                nullable: false,
                defaultValue: "data/logbook-attachments");

            migrationBuilder.CreateTable(
                name: "LogbookAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogbookEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogbookAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogbookAttachments_LogbookEntries_LogbookEntryId",
                        column: x => x.LogbookEntryId,
                        principalTable: "LogbookEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogbookAttachments_LogbookEntryId",
                table: "LogbookAttachments",
                column: "LogbookEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogbookAttachments");

            migrationBuilder.DropColumn(
                name: "LogbookAttachmentsDirectory",
                table: "OperationalSettings");
        }
    }
}
