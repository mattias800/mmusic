using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MusicGQL.Db.Models;
using MusicGQL.Db.Models.Events;
using MusicGQL.Db.Models.Events.ServerLibrary;
using MusicGQL.Db.Models.Projections;
using MusicGQL.Db.Models.ServerLibrary;
using MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

namespace MusicGQL.Db;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventCheckpoint> EventCheckpoints { get; set; }

    // Music library
    public DbSet<Artist> Artists { get; set; }
    public DbSet<Area> Areas { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Label> Labels { get; set; }
    public DbSet<Recording> Recordings { get; set; }
    public DbSet<Release> Releases { get; set; }
    public DbSet<ReleaseGroup> ReleaseGroups { get; set; }
    public DbSet<Work> Works { get; set; }

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

        modelBuilder.Entity<Artist>(artist =>
        {
            artist.OwnsOne(a => a.Rating, rating =>
            {
                rating.Property(r => r.Value).HasColumnName("RatingValue");
                rating.Property(r => r.VotesCount).HasColumnName("RatingVotesCount");
            });
            artist.OwnsOne(a => a.LifeSpan);
            artist.OwnsMany(a => a.Aliases);
            artist.OwnsMany(a => a.Relations, relation => 
            {
                relation.OwnsOne(r => r.Url);
            });
            artist.OwnsMany(a => a.Tags);
        });

        modelBuilder.Entity<ReleaseGroup>(rg =>
        {
            rg.OwnsOne(r => r.Rating, rating =>
            {
                rating.Property(r => r.Value).HasColumnName("RatingValue");
                rating.Property(r => r.VotesCount).HasColumnName("RatingVotesCount");
            });
            rg.OwnsMany(r => r.Aliases);
            rg.OwnsMany(r => r.Credits);
            rg.OwnsMany(r => r.Relations, relation => 
            {
                relation.OwnsOne(r => r.Url);
            });
            rg.OwnsMany(r => r.Tags);
        });

        modelBuilder.Entity<Recording>(recording =>
        {
            recording.OwnsOne(r => r.Rating, rating =>
            {
                rating.Property(r => r.Value).HasColumnName("RatingValue");
                rating.Property(r => r.VotesCount).HasColumnName("RatingVotesCount");
            });
            recording.OwnsMany(r => r.Aliases);
            recording.OwnsMany(r => r.Credits);
            recording.OwnsMany(r => r.Relations, relation => 
            {
                relation.OwnsOne(r => r.Url);
            });
            recording.OwnsMany(r => r.Tags);
        });

        modelBuilder.Entity<Release>(release =>
        {
            release.OwnsOne(r => r.CoverArtArchive);
            release.OwnsMany(r => r.Labels);
            release.OwnsMany(r => r.Media, media =>
            {
                media.OwnsMany(m => m.Discs);
                media.OwnsMany(m => m.Tracks);
            });
            release.OwnsMany(r => r.Credits);
            release.OwnsMany(r => r.Relations, relation => 
            {
                relation.OwnsOne(r => r.Url);
            });
            release.OwnsMany(r => r.Tags);
            release.OwnsOne(r => r.TextRepresentation);
            // We will add more for Release here as we process other types
        });

        modelBuilder.Entity<Work>(work =>
        {
            work.OwnsMany(w => w.Aliases);
            work.OwnsMany(w => w.Relations, relation =>
            {
                relation.OwnsOne(r => r.Url);
                // Note: Relation also has Artist and Work navigation properties.
                // Artist is an independent entity, Work is the current entity being configured.
                // This means a Relation owned by a Work can point back to the Work (or another Work) or an Artist.
            });
        });
    }
}
