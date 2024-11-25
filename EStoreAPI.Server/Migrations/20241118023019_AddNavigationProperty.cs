using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreAPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "deviceType",
                table: "Devices",
                newName: "DeviceType");

            migrationBuilder.RenameColumn(
                name: "deviceName",
                table: "Devices",
                newName: "DeviceName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceType",
                table: "Devices",
                newName: "deviceType");

            migrationBuilder.RenameColumn(
                name: "DeviceName",
                table: "Devices",
                newName: "deviceName");
        }
    }
}
