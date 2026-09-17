using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onudhabon.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyAssessmentsToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MonthlyAssessmentsJson",
                table: "Students",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyAssessmentsJson",
                table: "Students");
        }
    }
}
