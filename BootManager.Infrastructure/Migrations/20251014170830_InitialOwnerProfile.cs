using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialOwnerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OwnerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    HashAlgorithm = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RecoveryCodeHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    RecoveryCodeSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EncryptedProfilePayload = table.Column<byte[]>(type: "BLOB", nullable: false),
                    EncryptionVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerProfiles_Id",
                table: "OwnerProfiles",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnerProfiles");
        }
    }
}
