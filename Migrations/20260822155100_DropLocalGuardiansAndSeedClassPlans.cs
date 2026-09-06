using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 

namespace Onudhabon_ISD.Migrations
{
    public partial class DropLocalGuardiansAndSeedClassPlans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID(N'dbo.LocalGuardians', N'U') IS NOT NULL DROP TABLE dbo.LocalGuardians;");

            migrationBuilder.AlterColumn<string>(
                name: "Subjects",
                table: "ClassPlans",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "ClassPlans",
                columns: new[] { "Id", "ClassLevel", "CreatedAt", "Subjects", "__v" },
                values: new object[,]
                {
                    { 1, "1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla\",\"totalLectures\":10},{\"name\":\"English\",\"totalLectures\":10},{\"name\":\"Math\",\"totalLectures\":10}]", 0 },
                    { 2, "2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla\",\"totalLectures\":10},{\"name\":\"English\",\"totalLectures\":10},{\"name\":\"Math\",\"totalLectures\":10}]", 0 },
                    { 3, "3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla\",\"totalLectures\":10},{\"name\":\"English\",\"totalLectures\":10},{\"name\":\"Math\",\"totalLectures\":10}]", 0 },
                    { 4, "4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":12},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":12},{\"name\":\"English 1st paper\",\"totalLectures\":12},{\"name\":\"English 2nd paper\",\"totalLectures\":12},{\"name\":\"Math\",\"totalLectures\":12},{\"name\":\"Social Science\",\"totalLectures\":12},{\"name\":\"General Science\",\"totalLectures\":12}]", 0 },
                    { 5, "5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":12},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":12},{\"name\":\"English 1st paper\",\"totalLectures\":12},{\"name\":\"English 2nd paper\",\"totalLectures\":12},{\"name\":\"Math\",\"totalLectures\":12},{\"name\":\"Social Science\",\"totalLectures\":12},{\"name\":\"General Science\",\"totalLectures\":12}]", 0 },
                    { 6, "6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":12},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":12},{\"name\":\"English 1st paper\",\"totalLectures\":12},{\"name\":\"English 2nd paper\",\"totalLectures\":12},{\"name\":\"Math\",\"totalLectures\":12},{\"name\":\"Social Science\",\"totalLectures\":12},{\"name\":\"General Science\",\"totalLectures\":12}]", 0 },
                    { 7, "7", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":12},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":12},{\"name\":\"English 1st paper\",\"totalLectures\":12},{\"name\":\"English 2nd paper\",\"totalLectures\":12},{\"name\":\"Math\",\"totalLectures\":12},{\"name\":\"Social Science\",\"totalLectures\":12},{\"name\":\"General Science\",\"totalLectures\":12}]", 0 },
                    { 8, "8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":12},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":12},{\"name\":\"English 1st paper\",\"totalLectures\":12},{\"name\":\"English 2nd paper\",\"totalLectures\":12},{\"name\":\"Math\",\"totalLectures\":12},{\"name\":\"Social Science\",\"totalLectures\":12},{\"name\":\"General Science\",\"totalLectures\":12}]", 0 },
                    { 9, "9", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":12},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":12},{\"name\":\"English 1st paper\",\"totalLectures\":12},{\"name\":\"English 2nd paper\",\"totalLectures\":12},{\"name\":\"Math\",\"totalLectures\":12},{\"name\":\"Social Science\",\"totalLectures\":12},{\"name\":\"General Science\",\"totalLectures\":12},{\"name\":\"Physics\",\"totalLectures\":12},{\"name\":\"Chemistry\",\"totalLectures\":12},{\"name\":\"Higher Math\",\"totalLectures\":12},{\"name\":\"Biology\",\"totalLectures\":12}]", 0 },
                    { 10, "10", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":12},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":12},{\"name\":\"English 1st paper\",\"totalLectures\":12},{\"name\":\"English 2nd paper\",\"totalLectures\":12},{\"name\":\"Math\",\"totalLectures\":12},{\"name\":\"Social Science\",\"totalLectures\":12},{\"name\":\"General Science\",\"totalLectures\":12},{\"name\":\"Physics\",\"totalLectures\":12},{\"name\":\"Chemistry\",\"totalLectures\":12},{\"name\":\"Higher Math\",\"totalLectures\":12},{\"name\":\"Biology\",\"totalLectures\":12}]", 0 },
                    { 11, "11", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":20},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":20},{\"name\":\"English 1st paper\",\"totalLectures\":20},{\"name\":\"English 2nd paper\",\"totalLectures\":20},{\"name\":\"Physics 1st paper\",\"totalLectures\":20},{\"name\":\"Physics 2nd paper\",\"totalLectures\":20},{\"name\":\"Chemistry 1st paper\",\"totalLectures\":20},{\"name\":\"Chemistry 2nd paper\",\"totalLectures\":20},{\"name\":\"Higher Math 1st paper\",\"totalLectures\":20},{\"name\":\"Higher Math 2nd paper\",\"totalLectures\":20},{\"name\":\"Biology 1st paper\",\"totalLectures\":20},{\"name\":\"Biology 2nd paper\",\"totalLectures\":20}]", 0 },
                    { 12, "12", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"name\":\"Bangla 1st paper\",\"totalLectures\":20},{\"name\":\"Bangla 2nd paper\",\"totalLectures\":20},{\"name\":\"English 1st paper\",\"totalLectures\":20},{\"name\":\"English 2nd paper\",\"totalLectures\":20},{\"name\":\"Physics 1st paper\",\"totalLectures\":20},{\"name\":\"Physics 2nd paper\",\"totalLectures\":20},{\"name\":\"Chemistry 1st paper\",\"totalLectures\":20},{\"name\":\"Chemistry 2nd paper\",\"totalLectures\":20},{\"name\":\"Higher Math 1st paper\",\"totalLectures\":20},{\"name\":\"Higher Math 2nd paper\",\"totalLectures\":20},{\"name\":\"Biology 1st paper\",\"totalLectures\":20},{\"name\":\"Biology 2nd paper\",\"totalLectures\":20}]", 0 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ClassPlans",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.AlterColumn<string>(
                name: "Subjects",
                table: "ClassPlans",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LocalGuardians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Roles = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Local Guardian"),
                    __v = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalGuardians", x => x.Id);
                });
        }
    }
}