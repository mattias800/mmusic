using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Models;
using MusicGQL.Db.Models.Projections;
using Path = System.IO.Path;

namespace MusicGQL.Db;

public class EventDbContext : DbContext
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventCheckpoint> EventCheckpoints { get; set; }

    // Projections
    public DbSet<LikedSongsProjection> LikedSongsProjections { get; set; }

    public string DbPath { get; }

    public EventDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "blogging.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}