using Microsoft.EntityFrameworkCore;
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
    public DbSet<DbLikedSong> LikedSongs { get; set; }
    public DbSet<DbServerArtist> ServerArtists { get; set; }
    public DbSet<DbServerReleaseGroup> ServerReleaseGroups { get; set; }

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

        modelBuilder.Entity<DbUser>().ToTable("Users").HasKey(u => u.UserId);

        modelBuilder.Entity<DbPlaylist>(b =>
        {
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

        modelBuilder
            .Entity<DbPlaylist>()
            .HasMany(p => p.Items)
            .WithOne(i => i.Playlist)
            .HasForeignKey(i => i.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DbPlaylistItem>(b =>
        {
            b.HasKey(i => i.Id);

            b.Property(i => i.RecordingId).IsRequired();

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

        modelBuilder.Entity<DbServerArtist>(b =>
        {
            b.ToTable("ServerArtists");
            b.HasKey(sa => sa.Id);

            b.HasIndex(sa => sa.ArtistId).IsUnique();

            b.Property(sa => sa.ArtistId).IsRequired();

            b.HasOne(sa => sa.AddedBy)
                .WithMany()
                .HasForeignKey(sa => sa.AddedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DbServerReleaseGroup>(b =>
        {
            b.ToTable("ServerReleaseGroups");
            b.HasKey(srg => srg.Id);

            b.HasIndex(srg => srg.ReleaseGroupId).IsUnique();

            b.Property(srg => srg.ReleaseGroupId).IsRequired();

            b.HasOne(srg => srg.AddedBy)
                .WithMany()
                .HasForeignKey(srg => srg.AddedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
