using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options) { }

        public DbSet<Categories> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventEnrolment> EventEnrolments { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Routes> Routes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.Email).IsUnique();

            modelBuilder.Entity<Organiser>()
                .HasIndex(o => o.Email).IsUnique();

            modelBuilder.Entity<EventEnrolment>()
                .HasIndex(e => new { e.EventId, e.ParticipantId }).IsUnique();

            modelBuilder.Entity<Result>()
                .HasOne(r => r.EventEnrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EventEnrollmentId);
        }
    }
}