using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MusicGQL.Db.Models;
using MusicGQL.Db.Models.Events;
using MusicGQL.Db.Models.Projections;

namespace MusicGQL.Db;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventCheckpoint> EventCheckpoints { get; set; }

    // Projections
    public DbSet<LikedSongsProjection> LikedSongsProjections { get; set; }

    // Sagas
    public DbSet<Saga> Sagas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Event>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<LikedSong>("LikedSong")
            .HasValue<UnlikedSong>("UnlikedSong");

        modelBuilder
            .Entity<LikedSongsProjection>()
            .Property(p => p.LikedSongRecordingIds)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            )
            .Metadata.SetValueComparer(
                new ValueComparer<List<string>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                )
            );
    }
}