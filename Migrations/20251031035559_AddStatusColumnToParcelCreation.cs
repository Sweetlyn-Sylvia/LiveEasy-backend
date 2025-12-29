using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusColumnToParcelCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ParcelCreations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ParcelCreations");
        }
    }
}
