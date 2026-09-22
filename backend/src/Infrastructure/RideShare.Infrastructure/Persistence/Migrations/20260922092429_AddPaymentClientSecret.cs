using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideShare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentClientSecret : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientSecret",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "Payments");
        }
    }
}
