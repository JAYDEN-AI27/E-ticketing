using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class PaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cardnum = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ccv = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    expire_year = table.Column<int>(type: "int", maxLength: 2, nullable: false),
                    expire_month = table.Column<int>(type: "int", maxLength: 2, nullable: false),
                    status = table.Column<bool>(type: "bit", nullable: false),
                    MemberEmail = table.Column<string>(type: "nvarchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Users_MemberEmail",
                        column: x => x.MemberEmail,
                        principalTable: "Users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberEmail",
                table: "Payments",
                column: "MemberEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
