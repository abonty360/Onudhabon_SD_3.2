using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onudhabon_ISD.Migrations
{
    /// <inheritdoc />
    public partial class AddForumLikesCommentsAndRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorRole",
                table: "ForumPosts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ForumPosts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Dislikes",
                table: "ForumPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "ForumPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ForumComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AuthorRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumComments", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForumComments");

            migrationBuilder.DropColumn(
                name: "AuthorRole",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "Dislikes",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "Likes",
                table: "ForumPosts");
        }
    }
}
