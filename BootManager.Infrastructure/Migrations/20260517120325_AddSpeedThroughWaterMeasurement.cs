using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeedThroughWaterMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpeedThroughWaterMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecordedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    SpeedMetersPerSecond = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    SpeedKnots = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    SpeedWaterReferenceType = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeedThroughWaterMeasurements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpeedThroughWaterMeasurements_RecordedAtUtc",
                table: "SpeedThroughWaterMeasurements",
                column: "RecordedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeedThroughWaterMeasurements");
        }
    }
}
