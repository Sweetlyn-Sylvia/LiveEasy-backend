using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeliveryAmount",
                table: "ParcelCreations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "FastDelivery",
                table: "ParcelCreations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "ParcelCreations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMode",
                table: "ParcelCreations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAmount",
                table: "ParcelCreations");

            migrationBuilder.DropColumn(
                name: "FastDelivery",
                table: "ParcelCreations");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "ParcelCreations");

            migrationBuilder.DropColumn(
                name: "PaymentMode",
                table: "ParcelCreations");
        }
    }
}
