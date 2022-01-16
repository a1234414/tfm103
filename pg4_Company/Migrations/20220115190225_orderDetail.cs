using Microsoft.EntityFrameworkCore.Migrations;

namespace pg4_Company.Migrations
{
    public partial class orderDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "OrderDetail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "OrderDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
