using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class delete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Paid",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Subscriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                table: "Subscriptions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
