using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class NamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ccv",
                table: "Payments",
                newName: "Ccv");

            migrationBuilder.RenameColumn(
                name: "cardnum",
                table: "Payments",
                newName: "CardNum");

            migrationBuilder.RenameColumn(
                name: "expire_year",
                table: "Payments",
                newName: "Expired_year");

            migrationBuilder.RenameColumn(
                name: "expire_month",
                table: "Payments",
                newName: "Expired_month");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ccv",
                table: "Payments",
                newName: "ccv");

            migrationBuilder.RenameColumn(
                name: "CardNum",
                table: "Payments",
                newName: "cardnum");

            migrationBuilder.RenameColumn(
                name: "Expired_year",
                table: "Payments",
                newName: "expire_year");

            migrationBuilder.RenameColumn(
                name: "Expired_month",
                table: "Payments",
                newName: "expire_month");

            migrationBuilder.AddColumn<bool>(
                name: "status",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
