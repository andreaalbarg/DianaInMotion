using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dianainmotion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEventIsFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000001",
                column: "IsFeatured",
                value: false);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000002",
                column: "IsFeatured",
                value: false);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000003",
                column: "IsFeatured",
                value: false);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000004",
                column: "IsFeatured",
                value: true);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000005",
                column: "IsFeatured",
                value: true);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000006",
                column: "IsFeatured",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Events");
        }
    }
}
