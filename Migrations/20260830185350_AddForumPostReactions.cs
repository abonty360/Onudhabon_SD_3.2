using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onudhabon_ISD.Migrations
{
    /// <inheritdoc />
    public partial class AddForumPostReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForumPostReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsLike = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumPostReactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForumPostReactions_PostId_UserId",
                table: "ForumPostReactions",
                columns: new[] { "PostId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForumPostReactions");
        }
    }
}
