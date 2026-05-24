using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerSetupFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnboardingCompleted",
                table: "OwnerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PasswordChangeRequired",
                table: "OwnerProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnboardingCompleted",
                table: "OwnerProfiles");

            migrationBuilder.DropColumn(
                name: "PasswordChangeRequired",
                table: "OwnerProfiles");
        }
    }
}
