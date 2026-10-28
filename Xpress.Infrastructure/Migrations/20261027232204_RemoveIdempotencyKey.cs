using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xpress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Loans_IdempotencyKey",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Loans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Loans",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_IdempotencyKey",
                table: "Loans",
                column: "IdempotencyKey",
                unique: true);
        }
    }
}
