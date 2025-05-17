using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MusicGQL.Db.Models;
using MusicGQL.Db.Models.Events;
using MusicGQL.Db.Models.Events.ServerLibrary;
using MusicGQL.Db.Models.Projections;
using MusicGQL.Db.Models.ServerLibrary;

namespace MusicGQL.Db;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventCheckpoint> EventCheckpoints { get; set; }

    // Music library
    public DbSet<Artist> Artists { get; set; }
    public DbSet<Area> Areas { get; set; }

    // Projections
    public DbSet<LikedSongsProjection> LikedSongsProjections { get; set; }
    public DbSet<ArtistsAddedToServerLibraryProjection> ArtistsAddedToServerLibraryProjection { get; set; }
    public DbSet<ReleaseGroupsAddedToServerLibraryProjection> ReleaseGroupsAddedToServerLibraryProjection { get; set; }

    // Sagas
    public DbSet<Saga> Sagas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Event>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<LikedSong>("LikedSong")
            .HasValue<UnlikedSong>("UnlikedSong")
            .HasValue<AddArtistToServerLibrary>("AddArtistToServerLibrary")
            .HasValue<AddReleaseGroupToServerLibrary>("AddReleaseGroupToServerLibrary");

        // Do not include in migrations
        modelBuilder.Entity<Saga>().ToTable("sagas", t => t.ExcludeFromMigrations());

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

        modelBuilder
            .Entity<ArtistsAddedToServerLibraryProjection>()
            .Property(p => p.ArtistMbIds)
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

        modelBuilder
            .Entity<ReleaseGroupsAddedToServerLibraryProjection>()
            .Property(p => p.ReleaseGroupMbIds)
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
