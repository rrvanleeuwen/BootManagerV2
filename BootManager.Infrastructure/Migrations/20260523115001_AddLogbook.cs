using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogbook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogbookTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DepartureUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ArrivalUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeparturePort = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    DestinationPort = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    VesselName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Crew = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogbookTrips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogbookEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogbookTripId = table.Column<int>(type: "INTEGER", nullable: false),
                    EntryTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BaroPressure = table.Column<decimal>(type: "TEXT", precision: 7, scale: 2, nullable: true),
                    LogValue = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: true),
                    Course = table.Column<int>(type: "INTEGER", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    WindDescription = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    GpsStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogbookEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogbookEntries_LogbookTrips_LogbookTripId",
                        column: x => x.LogbookTripId,
                        principalTable: "LogbookTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogbookEntries_LogbookTripId_EntryTimeUtc",
                table: "LogbookEntries",
                columns: new[] { "LogbookTripId", "EntryTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LogbookTrips_DepartureUtc",
                table: "LogbookTrips",
                column: "DepartureUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogbookEntries");

            migrationBuilder.DropTable(
                name: "LogbookTrips");
        }
    }
}
