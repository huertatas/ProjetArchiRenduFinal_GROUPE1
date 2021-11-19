using Microsoft.EntityFrameworkCore.Migrations;

namespace Archi.API.Migrations
{
    public partial class LastTentative : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedDateDEUX",
                table: "Pizzas",
                newName: "LastTentative");

            migrationBuilder.RenameColumn(
                name: "CreatedDateDEUX",
                table: "Customers",
                newName: "LastTentative");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastTentative",
                table: "Pizzas",
                newName: "CreatedDateDEUX");

            migrationBuilder.RenameColumn(
                name: "LastTentative",
                table: "Customers",
                newName: "CreatedDateDEUX");
        }
    }
}
