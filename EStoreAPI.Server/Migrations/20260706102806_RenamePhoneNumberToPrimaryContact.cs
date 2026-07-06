using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreAPI.Server.Migrations
{
    public partial class RenamePhoneNumberToPrimaryContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "PhoneNumber", table: "Customers", newName: "PrimaryContact");
            migrationBuilder.RenameColumn(name: "PhoneNumberSecondary", table: "Customers", newName: "PhoneNumber");
            migrationBuilder.RenameIndex(name: "IX_Customers_PhoneNumber", table: "Customers", newName: "IX_Customers_PrimaryContact");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(name: "IX_Customers_PrimaryContact", table: "Customers", newName: "IX_Customers_PhoneNumber");
            migrationBuilder.RenameColumn(name: "PhoneNumber", table: "Customers", newName: "PhoneNumberSecondary");
            migrationBuilder.RenameColumn(name: "PrimaryContact", table: "Customers", newName: "PhoneNumber");
        }
    }
}
