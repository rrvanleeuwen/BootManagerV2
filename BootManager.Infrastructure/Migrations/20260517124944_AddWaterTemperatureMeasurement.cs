using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWaterTemperatureMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WaterTemperatureMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecordedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    TemperatureInstance = table.Column<byte>(type: "INTEGER", nullable: false),
                    TemperatureKelvin = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    TemperatureCelsius = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterTemperatureMeasurements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WaterTemperatureMeasurements_RecordedAtUtc",
                table: "WaterTemperatureMeasurements",
                column: "RecordedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WaterTemperatureMeasurements");
        }
    }
}
