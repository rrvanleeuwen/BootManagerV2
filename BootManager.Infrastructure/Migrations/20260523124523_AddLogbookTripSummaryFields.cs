using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogbookTripSummaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EngineHoursEnd",
                table: "LogbookTrips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EngineHoursStart",
                table: "LogbookTrips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fuel",
                table: "LogbookTrips",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LoggedMiles",
                table: "LogbookTrips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LogstandStart",
                table: "LogbookTrips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSailingHours",
                table: "LogbookTrips",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngineHoursEnd",
                table: "LogbookTrips");

            migrationBuilder.DropColumn(
                name: "EngineHoursStart",
                table: "LogbookTrips");

            migrationBuilder.DropColumn(
                name: "Fuel",
                table: "LogbookTrips");

            migrationBuilder.DropColumn(
                name: "LoggedMiles",
                table: "LogbookTrips");

            migrationBuilder.DropColumn(
                name: "LogstandStart",
                table: "LogbookTrips");

            migrationBuilder.DropColumn(
                name: "TotalSailingHours",
                table: "LogbookTrips");
        }
    }
}
