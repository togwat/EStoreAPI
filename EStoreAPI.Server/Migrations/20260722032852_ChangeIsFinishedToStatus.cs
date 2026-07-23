using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreAPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIsFinishedToStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "text",
                defaultValue: "InProgress",
                nullable: false);

            // IsFinished false -> InProgress, true -> Finished
            migrationBuilder.Sql("""
                UPDATE "Jobs" SET "Status" = 'Finished' WHERE "IsFinished";
                """);

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "Jobs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // any status other than Finished is false
            migrationBuilder.Sql("""
                UPDATE "Jobs" SET "IsFinished" = ("Status" = 'Finished');
                """);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Jobs");
        }
    }
}
