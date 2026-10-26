using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dianainmotion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTimezone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Events",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000001",
                column: "Timezone",
                value: "UTC");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000002",
                column: "Timezone",
                value: "UTC");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000003",
                column: "Timezone",
                value: "UTC");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000004",
                column: "Timezone",
                value: "UTC");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000005",
                column: "Timezone",
                value: "UTC");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000006",
                column: "Timezone",
                value: "UTC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Events");
        }
    }
}
