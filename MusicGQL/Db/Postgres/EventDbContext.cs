using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Likes.Db;
using MusicGQL.Features.Likes.Events;
using MusicGQL.Features.PlayCounts.Db;
using MusicGQL.Features.Playlists.Db;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Features.ServerSettings.Events;
using MusicGQL.Features.Users.Db;
using MusicGQL.Features.Users.Events;

namespace MusicGQL.Db.Postgres;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventCheckpoint> EventCheckpoints { get; set; }

    // Projections
    public DbSet<DbLikedSong> LikedSongs { get; set; }

    public DbSet<DbPlaylist> Playlists { get; set; }
    public DbSet<DbUser> Users { get; set; }

    public DbSet<DbTrackPlayCount> TrackPlayCounts { get; set; }
    public DbSet<DbUserTrackPlayCount> UserTrackPlayCounts { get; set; }

    public DbSet<DbServerSettings> ServerSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Event>()
            .HasDiscriminator<string>("Discriminator")
            // Map DB rows with Discriminator = "Event" to a concrete fallback type
            .HasValue<UnknownEvent>("Event")
            .HasValue<LikedSong>("LikedSong")
            .HasValue<UnlikedSong>("UnlikedSong")
            .HasValue<CreatedPlaylist>("CreatedPlaylist")
            .HasValue<RenamedPlaylist>("RenamedPlaylist")
            .HasValue<SongAddedToPlaylist>("SongAddedToPlaylist")
            .HasValue<UserCreated>("UserCreated")
            .HasValue<UserPasswordHashUpdated>("UserPasswordHashUpdated")
            .HasValue<UserListenBrainzCredentialsUpdated>("UserListenBrainzCredentialsUpdated")
            .HasValue<MusicGQL.Features.Users.Events.UserRolesUpdated>("UserRolesUpdated")
            .HasValue<LibraryPathUpdated>("LibraryPathUpdated");
        modelBuilder.Entity<DbTrackPlayCount>(b =>
        {
            b.ToTable("TrackPlayCounts");
            b.HasKey(x => x.Id);
            b.Property(x => x.ArtistId).IsRequired();
            b.Property(x => x.ReleaseFolderName).IsRequired();
            b.Property(x => x.TrackNumber).IsRequired();
            b.HasIndex(x => new
                {
                    x.ArtistId,
                    x.ReleaseFolderName,
                    x.TrackNumber,
                })
                .IsUnique();
        });

        modelBuilder.Entity<DbUserTrackPlayCount>(b =>
        {
            b.ToTable("UserTrackPlayCounts");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.ArtistId).IsRequired();
            b.Property(x => x.ReleaseFolderName).IsRequired();
            b.Property(x => x.TrackNumber).IsRequired();
            b.HasIndex(x => new
                {
                    x.UserId,
                    x.ArtistId,
                    x.ReleaseFolderName,
                    x.TrackNumber,
                })
                .IsUnique();
        });

        modelBuilder.Entity<DbUser>().ToTable("Users").HasKey(u => u.UserId);

        modelBuilder.Entity<DbPlaylist>(b =>
        {
            b.ToTable("Playlists");
            b.HasKey(p => p.Id);

            b.Property(p => p.Name).HasMaxLength(255);
            b.Property(p => p.Description).HasMaxLength(1000);

            b.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(p => p.Items)
                .WithOne(i => i.Playlist)
                .HasForeignKey(i => i.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DbPlaylistItem>(b =>
        {
            b.ToTable("PlaylistItems");
            b.HasKey(i => i.Id);

            b.HasOne(i => i.Playlist)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DbLikedSong>(b =>
        {
            b.HasKey(ls => ls.Id);

            b.Property(ls => ls.RecordingId).IsRequired();

            b.HasOne(ls => ls.LikedBy)
                .WithMany()
                .HasForeignKey(ls => ls.LikedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Enforce one like per song per user
            b.HasIndex(ls => new { ls.LikedByUserId, ls.RecordingId }).IsUnique();

            // Optional: for query speed
            b.HasIndex(ls => ls.LikedByUserId);
        });

        modelBuilder.Entity<DbServerSettings>(b =>
        {
            b.ToTable("ServerSettings");
            b.HasKey(srg => srg.Id);
        });
    }
}
