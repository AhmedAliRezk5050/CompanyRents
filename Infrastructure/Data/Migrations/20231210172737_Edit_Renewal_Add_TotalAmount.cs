using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class Edit_Renewal_Add_TotalAmount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TaxRatio",
                table: "Renewals",
                type: "decimal(17,2)",
                precision: 17,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Renewals",
                type: "decimal(17,2)",
                precision: 17,
                scale: 2,
                nullable: false,
                computedColumnSql: "[Amount] + ([Amount] * [TaxRatio])");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Renewals");

            migrationBuilder.AlterColumn<double>(
                name: "TaxRatio",
                table: "Renewals",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(17,2)",
                oldPrecision: 17,
                oldScale: 2);
        }
    }
}
