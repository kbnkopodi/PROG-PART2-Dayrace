using Microsoft.EntityFrameworkCore;
using RaceDay.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RaceDay.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<RaceRoute> Routes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("USERS");
            modelBuilder.Entity<Event>().ToTable("EVENT");
            modelBuilder.Entity<Category>().ToTable("CATEGORY");
            modelBuilder.Entity<Entry>().ToTable("ENTRIES");
            modelBuilder.Entity<Result>().ToTable("RESULTS");
            modelBuilder.Entity<RaceRoute>().ToTable("ROUTES");
            modelBuilder.Entity<RaceRoute>().HasKey(r => r.RouteId);
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnName("PASSWORD");

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId);

            modelBuilder.Entity<Entry>()
                .HasOne(en => en.Participant)
                .WithMany(u => u.Entries)
                .HasForeignKey(en => en.UserId);

            modelBuilder.Entity<Entry>()
                .HasOne(en => en.Category)
                .WithMany(c => c.Entries)
                .HasForeignKey(en => en.CategoryId);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Entry)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EntryId);

            modelBuilder.Entity<RaceRoute>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Routes)
                .HasForeignKey(r => r.EventId);
        }
    }
}