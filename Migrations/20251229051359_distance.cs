using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class distance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ReceiverLatitude",
                table: "ParcelCreations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ReceiverLongitude",
                table: "ParcelCreations",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverLatitude",
                table: "ParcelCreations");

            migrationBuilder.DropColumn(
                name: "ReceiverLongitude",
                table: "ParcelCreations");
        }
    }
}
