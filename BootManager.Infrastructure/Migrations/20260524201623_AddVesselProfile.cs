using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVesselProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VesselProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VesselName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    HomePort = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    CallSign = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Mmsi = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VesselProfiles_CreatedUtc",
                table: "VesselProfiles",
                column: "CreatedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VesselProfiles");
        }
    }
}
