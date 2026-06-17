using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateOwnerProfileToLocalUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    HashAlgorithm = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CredentialVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordChangeRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    OnboardingCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    EncryptedProfilePayload = table.Column<byte[]>(type: "BLOB", nullable: false),
                    EncryptionVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    PinHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PinSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    RecoveryCodeHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    RecoveryCodeSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalUsers_NormalizedName",
                table: "LocalUsers",
                column: "NormalizedName",
                unique: true);

            // Migrate data from OwnerProfiles to LocalUsers if data exists
            migrationBuilder.Sql(
                @"INSERT INTO LocalUsers (
                    Id, DisplayName, NormalizedName, Role, IsActive,
                    PasswordHash, PasswordSalt, HashAlgorithm, CredentialVersion,
                    PasswordChangeRequired, OnboardingCompleted,
                    EncryptedProfilePayload, EncryptionVersion,
                    PinHash, PinSalt, RecoveryCodeHash, RecoveryCodeSalt,
                    CreatedUtc, UpdatedUtc
                )
                SELECT
                    Id, 'Owner', 'owner', 0, 1,
                    PasswordHash, PasswordSalt, HashAlgorithm, 1,
                    PasswordChangeRequired, OnboardingCompleted,
                    EncryptedProfilePayload, EncryptionVersion,
                    PinHash, PinSalt, RecoveryCodeHash, RecoveryCodeSalt,
                    CreatedUtc, UpdatedUtc
                FROM OwnerProfiles
                WHERE Id IS NOT NULL");

            migrationBuilder.DropTable(name: "OwnerProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalUsers");

            migrationBuilder.CreateTable(
                name: "OwnerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EncryptedProfilePayload = table.Column<byte[]>(type: "BLOB", nullable: false),
                    EncryptionVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    HashAlgorithm = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OnboardingCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordChangeRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PinHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PinSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    RecoveryCodeHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    RecoveryCodeSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
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
    }
}
