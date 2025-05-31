using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Likes.Db;
using MusicGQL.Features.Likes.Events;
using MusicGQL.Features.Playlists.Db;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Features.ServerLibrary.Events;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using MusicGQL.Features.Users.Db;
using MusicGQL.Features.Users.Events;

namespace MusicGQL.Db.Postgres;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventCheckpoint> EventCheckpoints { get; set; }

    // Projections
    public DbSet<LikedSongsProjection> LikedSongsProjections { get; set; }
    public DbSet<ArtistsAddedToServerLibraryProjection> ArtistsAddedToServerLibraryProjections { get; set; }
    public DbSet<ReleaseGroupsAddedToServerLibraryProjection> ReleaseGroupsAddedToServerLibraryProjections { get; set; }

    public DbSet<DbPlaylist> Playlists { get; set; }
    public DbSet<DbUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Event>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<LikedSong>("LikedSong")
            .HasValue<UnlikedSong>("UnlikedSong")
            .HasValue<AddArtistToServerLibrary>("AddArtistToServerLibrary")
            .HasValue<AddReleaseGroupToServerLibrary>("AddReleaseGroupToServerLibrary")
            .HasValue<CreatedPlaylist>("CreatedPlaylist")
            .HasValue<RenamedPlaylist>("RenamedPlaylist")
            .HasValue<SongAddedToPlaylist>("SongAddedToPlaylist")
            .HasValue<UserCreated>("UserCreated")
            .HasValue<UserPasswordHashSet>("UserPasswordHashSet");

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

        modelBuilder.Entity<DbUser>().ToTable("Users").HasKey(u => u.UserId);

        modelBuilder.Entity<DbPlaylist>().ToTable("Playlists").HasKey(p => p.Id);

        modelBuilder
            .Entity<DbPlaylist>()
            .HasMany(p => p.Items)
            .WithOne(i => i.Playlist)
            .HasForeignKey(i => i.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DbPlaylistItem>().ToTable("PlaylistItems").HasKey(i => i.Id);
    }
}
