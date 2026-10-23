using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dianainmotion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedRecordingUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000004",
                column: "RecordingUrl",
                value: "https://www.youtube.com/watch?v=dQw4w9WgXcQ");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000005",
                column: "RecordingUrl",
                value: "https://www.youtube.com/watch?v=jNQXAC9IVRw");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000006",
                column: "RecordingUrl",
                value: "https://www.youtube.com/watch?v=9bZkp7q19f0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000004",
                column: "RecordingUrl",
                value: "https://www.youtube.com/watch?v=placeholder1");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000005",
                column: "RecordingUrl",
                value: "https://www.youtube.com/watch?v=placeholder2");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-0000-0000-0000-000000000006",
                column: "RecordingUrl",
                value: "https://www.youtube.com/watch?v=placeholder3");
        }
    }
}
