using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIFinancePlatform.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPriceCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductPriceCaches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchTerm = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Price = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceCaches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceCaches_SearchTerm",
                table: "ProductPriceCaches",
                column: "SearchTerm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPriceCaches");
        }
    }
}
