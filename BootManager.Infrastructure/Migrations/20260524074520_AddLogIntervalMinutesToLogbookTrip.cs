using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogIntervalMinutesToLogbookTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LogIntervalMinutes",
                table: "LogbookTrips",
                type: "INTEGER",
                nullable: false,
                defaultValue: 60);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogIntervalMinutes",
                table: "LogbookTrips");
        }
    }
}
