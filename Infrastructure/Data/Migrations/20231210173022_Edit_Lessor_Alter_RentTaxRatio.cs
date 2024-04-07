using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class Edit_Lessor_Alter_RentTaxRatio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalRentAmount",
                table: "Lessors",
                type: "decimal(17,2)",
                precision: 17,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(17,2)",
                oldPrecision: 17,
                oldScale: 2,
                oldComputedColumnSql: "[RentAmount] + ([RentAmount] * [RentTaxRatio])");

            migrationBuilder.AlterColumn<decimal>(
                name: "RentTaxRatio",
                table: "Lessors",
                type: "decimal(17,2)",
                precision: 17,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "RentTaxRatio",
                table: "Lessors",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(17,2)",
                oldPrecision: 17,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalRentAmount",
                table: "Lessors",
                type: "decimal(17,2)",
                precision: 17,
                scale: 2,
                nullable: false,
                computedColumnSql: "[RentAmount] + ([RentAmount] * [RentTaxRatio])",
                oldClrType: typeof(decimal),
                oldType: "decimal(17,2)",
                oldPrecision: 17,
                oldScale: 2);
        }
    }
}
