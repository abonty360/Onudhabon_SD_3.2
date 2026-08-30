using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onudhabon_ISD.Migrations
{
    /// <inheritdoc />
    public partial class AddForumPostStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ForumPosts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ForumPosts");
        }
    }
}
