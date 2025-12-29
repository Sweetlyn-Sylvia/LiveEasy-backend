using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryTimeToParcelCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryTime",
                table: "ParcelCreations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryTime",
                table: "ParcelCreations");
        }
    }
}
