using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreAPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddPartsPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PartsPrice",
                table: "Problems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            // The old Price column represented partsPrice.
            // In this migration, move the old Price column to partsPrice and leave Price at 0
            migrationBuilder.Sql(@"UPDATE ""Problems"" SET ""PartsPrice"" = ""Price"", ""Price"" = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore Price from PartsPrice before dropping the column
            migrationBuilder.Sql(@"UPDATE ""Problems"" SET ""Price"" = ""PartsPrice"";");

            migrationBuilder.DropColumn(
                name: "PartsPrice",
                table: "Problems");
        }
    }
}
