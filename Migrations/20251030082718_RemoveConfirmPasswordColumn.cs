using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelTrackingSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConfirmPasswordColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmPassword",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmPassword",
                table: "Agents");
        }
    }
}
