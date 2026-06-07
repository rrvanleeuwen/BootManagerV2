using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogbookTripLogstandEnd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LogstandEnd",
                table: "LogbookTrips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE LogbookTrips
                SET LogstandEnd = CAST(
                    CAST(LogstandStart AS REAL) + CAST(LoggedMiles AS REAL)
                    AS TEXT)
                WHERE LogstandStart IS NOT NULL
                  AND LoggedMiles IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogstandEnd",
                table: "LogbookTrips");
        }
    }
}
