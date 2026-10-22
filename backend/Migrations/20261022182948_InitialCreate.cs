using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace dianainmotion.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Speaker = table.Column<string>(type: "TEXT", nullable: true),
                    RecordingUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EventId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "RecordingUrl", "ScheduledAt", "Speaker", "Title" },
                values: new object[,]
                {
                    { "aaaaaaaa-0000-0000-0000-000000000001", "basics", new DateTime(2026, 10, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Aprende los fundamentos de Diana: cómo crear tu primera app, los flujos de trabajo esenciales y los building blocks de la plataforma.", null, new DateTime(2026, 11, 5, 18, 0, 0, 0, DateTimeKind.Utc), "Diana Team", "Getting Started with Diana" },
                    { "aaaaaaaa-0000-0000-0000-000000000002", "advanced", new DateTime(2026, 10, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Construye una aplicación completa de punta a punta: frontend, backend y base de datos, todo desde cero con Diana.", null, new DateTime(2026, 11, 12, 18, 0, 0, 0, DateTimeKind.Utc), "Diana Team", "Building Your First Full-Stack App" },
                    { "aaaaaaaa-0000-0000-0000-000000000003", "advanced", new DateTime(2026, 10, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Aprende a conectar tu app con servicios externos: pagos, autenticación, APIs de terceros y más.", null, new DateTime(2026, 11, 19, 18, 0, 0, 0, DateTimeKind.Utc), "Diana Team", "Integrations & External APIs" },
                    { "aaaaaaaa-0000-0000-0000-000000000004", "basics", new DateTime(2026, 9, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Una introducción general a lo que puedes construir con Diana. Exploramos casos de uso reales y mostramos el potencial de la plataforma.", "https://www.youtube.com/watch?v=placeholder1", new DateTime(2026, 9, 17, 18, 0, 0, 0, DateTimeKind.Utc), "Diana Team", "Intro to Diana: What Can You Build?" },
                    { "aaaaaaaa-0000-0000-0000-000000000005", "basics", new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cómo modelar datos, crear migraciones y conectar tu backend a una base de datos usando EF Core y SQLite.", "https://www.youtube.com/watch?v=placeholder2", new DateTime(2026, 10, 1, 18, 0, 0, 0, DateTimeKind.Utc), "Diana Team", "Working with Databases in Diana" },
                    { "aaaaaaaa-0000-0000-0000-000000000006", "advanced", new DateTime(2026, 10, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Aprende el flujo de deployment en Diana: preview, staging, producción y cómo gestionar variables de entorno y datos persistentes.", "https://www.youtube.com/watch?v=placeholder3", new DateTime(2026, 10, 15, 18, 0, 0, 0, DateTimeKind.Utc), "Diana Team", "Deploying & Shipping Your Diana App" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_EventId",
                table: "Registrations",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
