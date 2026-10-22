using dianainmotion.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace dianainmotion.Backend.Infrastructure;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        b.Properties<Guid>()
         .HaveConversion<GuidToStringConverter>();
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Registration>()
            .HasOne(r => r.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(r => r.EventId);

        mb.Entity<Event>().HasData(
            new Event
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000001"),
                Title = "Getting Started with Diana",
                Description = "Aprende los fundamentos de Diana: cómo crear tu primera app, los flujos de trabajo esenciales y los building blocks de la plataforma.",
                Category = "basics",
                ScheduledAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc),
                Speaker = "Diana Team",
                SpeakerPhotoUrl = null,
                RecordingUrl = null,
                CreatedAt = new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc),
            },
            new Event
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000002"),
                Title = "Building Your First Full-Stack App",
                Description = "Construye una aplicación completa de punta a punta: frontend, backend y base de datos, todo desde cero con Diana.",
                Category = "advanced",
                ScheduledAt = new DateTime(2026, 11, 12, 18, 0, 0, DateTimeKind.Utc),
                Speaker = "Diana Team",
                SpeakerPhotoUrl = null,
                RecordingUrl = null,
                CreatedAt = new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc),
            },
            new Event
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000003"),
                Title = "Integrations & External APIs",
                Description = "Aprende a conectar tu app con servicios externos: pagos, autenticación, APIs de terceros y más.",
                Category = "advanced",
                ScheduledAt = new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc),
                Speaker = "Diana Team",
                SpeakerPhotoUrl = null,
                RecordingUrl = null,
                CreatedAt = new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc),
            },
            new Event
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000004"),
                Title = "Intro to Diana: What Can You Build?",
                Description = "Una introducción general a lo que puedes construir con Diana. Exploramos casos de uso reales y mostramos el potencial de la plataforma.",
                Category = "basics",
                ScheduledAt = new DateTime(2026, 9, 17, 18, 0, 0, DateTimeKind.Utc),
                Speaker = "Diana Team",
                SpeakerPhotoUrl = null,
                RecordingUrl = "https://www.youtube.com/watch?v=placeholder1",
                CreatedAt = new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc),
            },
            new Event
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000005"),
                Title = "Working with Databases in Diana",
                Description = "Cómo modelar datos, crear migraciones y conectar tu backend a una base de datos usando EF Core y SQLite.",
                Category = "basics",
                ScheduledAt = new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
                Speaker = "Diana Team",
                SpeakerPhotoUrl = null,
                RecordingUrl = "https://www.youtube.com/watch?v=placeholder2",
                CreatedAt = new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
            },
            new Event
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000006"),
                Title = "Deploying & Shipping Your Diana App",
                Description = "Aprende el flujo de deployment en Diana: preview, staging, producción y cómo gestionar variables de entorno y datos persistentes.",
                Category = "advanced",
                ScheduledAt = new DateTime(2026, 10, 15, 18, 0, 0, DateTimeKind.Utc),
                Speaker = "Diana Team",
                SpeakerPhotoUrl = null,
                RecordingUrl = "https://www.youtube.com/watch?v=placeholder3",
                CreatedAt = new DateTime(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc),
            }
        );
    }
}
