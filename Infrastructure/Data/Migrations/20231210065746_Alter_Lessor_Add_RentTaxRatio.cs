using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class Alter_Lessor_Add_RentTaxRatio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RentPeriod",
                table: "Lessors",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRentAmount",
                table: "Lessors",
                type: "decimal(17,2)",
                precision: 17,
                scale: 2,
                nullable: false,
                computedColumnSql: "[RentAmount] + ([RentAmount] * [RentTaxRatio])");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRentAmount",
                table: "Lessors");

            migrationBuilder.AlterColumn<double>(
                name: "RentPeriod",
                table: "Lessors",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
