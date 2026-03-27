using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBatteryMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatteryMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecordedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Voltage = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    StateOfCharge = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatteryMeasurements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatteryMeasurements_RecordedAtUtc",
                table: "BatteryMeasurements",
                column: "RecordedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatteryMeasurements");
        }
    }
}
