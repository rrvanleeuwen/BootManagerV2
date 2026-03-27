using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PositionMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecordedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Latitude = table.Column<decimal>(type: "TEXT", precision: 10, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "TEXT", precision: 11, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionMeasurements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PositionMeasurements_RecordedAtUtc",
                table: "PositionMeasurements",
                column: "RecordedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PositionMeasurements");
        }
    }
}
