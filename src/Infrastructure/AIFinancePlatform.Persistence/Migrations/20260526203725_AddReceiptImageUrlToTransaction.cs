using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIFinancePlatform.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptImageUrlToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptImageUrl",
                table: "Transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptImageUrl",
                table: "Transactions");
        }
    }
}
