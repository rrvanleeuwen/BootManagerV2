using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationalSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ListenAddress = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ListenPort = table.Column<int>(type: "INTEGER", nullable: false),
                    AlternativeListenPort = table.Column<int>(type: "INTEGER", nullable: true),
                    ApiBaseUrl = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    RawStorageMode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DefaultSampleIntervalSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    CaptureLoggingEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationalSettings");
        }
    }
}
