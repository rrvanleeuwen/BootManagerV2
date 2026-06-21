using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BootManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMutationsAndExpectedLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExpectedLocationId",
                table: "Stocks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockMutations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MutationType = table.Column<int>(type: "INTEGER", nullable: false),
                    OldQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NewQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MutatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMutations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMutations_LocalUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "LocalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMutations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockMutations_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ExpectedLocationId",
                table: "Stocks",
                column: "ExpectedLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutations_MutatedAt",
                table: "StockMutations",
                column: "MutatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_StockMutations_ProductId",
                table: "StockMutations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutations_StorageLocationId",
                table: "StockMutations",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutations_UserId",
                table: "StockMutations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_StorageLocations_ExpectedLocationId",
                table: "Stocks",
                column: "ExpectedLocationId",
                principalTable: "StorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_StorageLocations_ExpectedLocationId",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "StockMutations");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_ExpectedLocationId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "ExpectedLocationId",
                table: "Stocks");
        }
    }
}
