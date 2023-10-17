using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferencePlanner.GraphQL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Session> Sessions { get; set; } = default!;

        public DbSet<Track> Tracks { get; set; } = default!;

        public DbSet<Speaker> Speakers { get; set; } = default!;

        public DbSet<Attendee> Attendees { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            IndexBuilder<Attendee> foo =
                modelBuilder
                    .Entity<Attendee>()
                    .HasIndex(a => a.UserName)
                    .IsUnique();

            // Many-to-many: Session <-> Attendee
            modelBuilder
                .Entity<SessionAttendee>()
                .HasKey(ca => new
                {
                    ca.SessionId, ca.AttendeeId,
                });

            // Many-to-many: Speaker <-> Session
            modelBuilder
                .Entity<SessionSpeaker>()
                .HasKey(ss => new
                {
                    ss.SessionId, ss.SpeakerId,
                });
        }
    }
}
