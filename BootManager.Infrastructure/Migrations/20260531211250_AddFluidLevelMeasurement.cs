using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFluidLevelMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FluidLevelMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecordedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Pgn = table.Column<uint>(type: "INTEGER", nullable: false),
                    GatewaySentence = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SourceAddress = table.Column<byte>(type: "INTEGER", nullable: true),
                    FluidInstance = table.Column<byte>(type: "INTEGER", nullable: false),
                    FluidType = table.Column<byte>(type: "INTEGER", nullable: false),
                    RawFluidType = table.Column<byte>(type: "INTEGER", nullable: false),
                    LevelPercent = table.Column<decimal>(type: "decimal(5, 2)", nullable: true),
                    CapacityLiters = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    IsLevelInvalid = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FluidLevelMeasurements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FluidLevelMeasurements_FluidType",
                table: "FluidLevelMeasurements",
                column: "FluidType");

            migrationBuilder.CreateIndex(
                name: "IX_FluidLevelMeasurements_FluidType_FluidInstance",
                table: "FluidLevelMeasurements",
                columns: new[] { "FluidType", "FluidInstance" });

            migrationBuilder.CreateIndex(
                name: "IX_FluidLevelMeasurements_RecordedAtUtc",
                table: "FluidLevelMeasurements",
                column: "RecordedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FluidLevelMeasurements");
        }
    }
}
