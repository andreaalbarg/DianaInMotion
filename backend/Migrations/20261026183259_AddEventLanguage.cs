using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dianainmotion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Events",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000001",
                column: "Language",
                value: "es");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000002",
                column: "Language",
                value: "es");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000003",
                column: "Language",
                value: "es");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000004",
                column: "Language",
                value: "es");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000005",
                column: "Language",
                value: "es");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000006",
                column: "Language",
                value: "es");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Events");
        }
    }
}
