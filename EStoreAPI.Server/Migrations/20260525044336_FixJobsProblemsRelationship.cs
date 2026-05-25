using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreAPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixJobsProblemsRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Problems_Jobs_JobId",
                table: "Problems");

            migrationBuilder.DropIndex(
                name: "IX_Problems_JobId",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Problems");

            migrationBuilder.CreateTable(
                name: "JobProblems",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    ProblemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobProblems", x => new { x.JobId, x.ProblemId });
                    table.ForeignKey(
                        name: "FK_JobProblems_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobProblems_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobProblems_ProblemId",
                table: "JobProblems",
                column: "ProblemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobProblems");

            migrationBuilder.AddColumn<int>(
                name: "JobId",
                table: "Problems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Problems_JobId",
                table: "Problems",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Problems_Jobs_JobId",
                table: "Problems",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "JobId");
        }
    }
}
