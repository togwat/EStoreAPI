using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreAPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class SplitPhoneNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumbers",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumberSecondary",
                table: "Customers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhoneNumberSecondary",
                table: "Customers");

            migrationBuilder.AddColumn<string[]>(
                name: "PhoneNumbers",
                table: "Customers",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }
    }
}
