using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class Alter_Lessor_Edit_RentTaxRatio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRentAmount",
                table: "Lessors");

            migrationBuilder.AlterColumn<decimal>(
                name: "RentTaxRatio",
                table: "Lessors",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "RentTaxRatio",
                table: "Lessors",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRentAmount",
                table: "Lessors",
                type: "decimal(17,2)",
                precision: 17,
                scale: 2,
                nullable: false,
                computedColumnSql: "[RentAmount] + ([RentAmount] * [RentTaxRatio])");
        }
    }
}
