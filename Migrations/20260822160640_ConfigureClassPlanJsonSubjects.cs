using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 

namespace Onudhabon_ISD.Migrations
{
    public partial class ConfigureClassPlanJsonSubjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Subjects",
                table: "ClassPlans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Subjects",
                table: "ClassPlans",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
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
    }
}