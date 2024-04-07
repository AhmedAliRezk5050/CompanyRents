using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class Alter_Lessors_Add_DateStrings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractDateString",
                table: "Lessors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RentEndDateString",
                table: "Lessors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RentStartDateString",
                table: "Lessors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractDateString",
                table: "Lessors");

            migrationBuilder.DropColumn(
                name: "RentEndDateString",
                table: "Lessors");

            migrationBuilder.DropColumn(
                name: "RentStartDateString",
                table: "Lessors");
        }
    }
}
