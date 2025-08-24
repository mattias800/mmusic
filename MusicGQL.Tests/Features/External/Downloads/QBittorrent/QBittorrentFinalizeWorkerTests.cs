using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.QBittorrent;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Reader;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Features.ServerSettings.Db;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MusicGQL.Tests.Features.External.Downloads.QBittorrent;

public class QBittorrentFinalizeWorkerTests
{
    private sealed class DummyEventSender : HotChocolate.Subscriptions.ITopicEventSender
    {
        public ValueTask SendAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask CompleteAsync(string topic) => ValueTask.CompletedTask;
    }

    private sealed class FakeFinalizeService : IQBittorrentFinalizeService
    {
        public int Calls { get; private set; }
        public List<(string artistId, string folder)> Invocations { get; } = new();
        public Task<bool> FinalizeReleaseAsync(string artistId, string releaseFolderName, CancellationToken ct)
        {
            Calls++;
            Invocations.Add((artistId, releaseFolderName));
            return Task.FromResult(false);
        }
    }

    private sealed class TestDbFactory : Microsoft.EntityFrameworkCore.IDbContextFactory<MusicGQL.Db.Postgres.EventDbContext>
    {
        private readonly DbContextOptions<MusicGQL.Db.Postgres.EventDbContext> _options;
        public TestDbFactory(DbContextOptions<MusicGQL.Db.Postgres.EventDbContext> options) => _options = options;
        public MusicGQL.Db.Postgres.EventDbContext CreateDbContext() => new MusicGQL.Db.Postgres.EventDbContext(_options);
    }

    private static Microsoft.EntityFrameworkCore.IDbContextFactory<MusicGQL.Db.Postgres.EventDbContext> CreateDbFactoryWithSettings(string libraryPath, string logsPath)
    {
        var options = new DbContextOptionsBuilder<MusicGQL.Db.Postgres.EventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var factory = new TestDbFactory(options);
        using var db = factory.CreateDbContext();
        db.ServerSettings.Add(new DbServerSettings
        {
            Id = DefaultDbServerSettingsProvider.ServerSettingsSingletonId,
            LibraryPath = libraryPath,
            LogsFolderPath = logsPath,
            EnableQBittorrentDownloader = true
        });
        db.SaveChanges();
        return factory;
    }

    private static void CreateLibraryWithMissingRelease(string root, string artistId, string releaseFolder, string artistName, string releaseTitle)
    {
        var artistDir = Path.Combine(root, artistId);
        Directory.CreateDirectory(artistDir);
        var releaseDir = Path.Combine(artistDir, releaseFolder);
        Directory.CreateDirectory(releaseDir);
        File.WriteAllText(Path.Combine(artistDir, "artist.json"), "{\"id\":\"" + artistId + "\",\"name\":\"" + artistName + "\"}");
        // No audio files referenced or present -> tracks will be Missing
        File.WriteAllText(Path.Combine(releaseDir, "release.json"), "{\"title\":\"" + releaseTitle + "\",\"artistName\":\"" + artistName + "\",\"tracks\":[{\"title\":\"A\",\"trackNumber\":1},{\"title\":\"B\",\"trackNumber\":2}]}");
    }

    [Fact]
    public async Task Worker_ScanOnce_Attempts_Finalize_For_Missing_Releases()
    {
        // Arrange
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        try
        {
            CreateLibraryWithMissingRelease(tmp, "Artist", "Album", "Artist", "Album");
            var dbFactory = CreateDbFactoryWithSettings(tmp, Path.GetTempPath());
            var settings = new ServerSettingsAccessor(dbFactory);
            var cache = new ServerLibraryCache(new ServerLibraryJsonReader(settings), new DummyEventSender());
            var fakeFinalize = new FakeFinalizeService();
            var logProvider = new MusicGQL.Features.Downloads.Services.DownloadLogPathProvider(settings);

            var worker = new QBittorrentFinalizeWorker(
                Options.Create(new QBittorrentFinalizeWorkerOptions
                {
                    Enabled = true,
                    ScanIntervalMinutes = 60,
                    AttemptCooldownMinutes = 10,
                    MaxReleasesPerScan = 10
                }),
                NullLogger<QBittorrentFinalizeWorker>.Instance,
                cache,
                fakeFinalize,
                logProvider
            );

            // Act
            var attempts = await worker.ScanOnceAsync(CancellationToken.None);

            // Assert
            Assert.Equal(1, attempts);
            Assert.Equal(1, fakeFinalize.Calls);
            Assert.Contains(fakeFinalize.Invocations, x => x.artistId == "Artist" && x.folder == "Album");
        }
        finally
        {
            try { Directory.Delete(tmp, true); } catch { }
        }
    }

    [Fact]
    public async Task Worker_Respects_Attempt_Cooldown()
    {
        // Arrange
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        try
        {
            CreateLibraryWithMissingRelease(tmp, "Artist", "Album", "Artist", "Album");
            var dbFactory = CreateDbFactoryWithSettings(tmp, Path.GetTempPath());
            var settings = new ServerSettingsAccessor(dbFactory);
            var cache = new ServerLibraryCache(new ServerLibraryJsonReader(settings), new DummyEventSender());
            var fakeFinalize = new FakeFinalizeService();
            var logProvider = new MusicGQL.Features.Downloads.Services.DownloadLogPathProvider(settings);

            var worker = new QBittorrentFinalizeWorker(
                Options.Create(new QBittorrentFinalizeWorkerOptions
                {
                    Enabled = true,
                    ScanIntervalMinutes = 60,
                    AttemptCooldownMinutes = 60,
                    MaxReleasesPerScan = 10
                }),
                NullLogger<QBittorrentFinalizeWorker>.Instance,
                cache,
                fakeFinalize,
                logProvider
            );

            // Act
            var attempts1 = await worker.ScanOnceAsync(CancellationToken.None);
            var attempts2 = await worker.ScanOnceAsync(CancellationToken.None); // immediately again, within cooldown

            // Assert
            Assert.Equal(1, attempts1);
            Assert.Equal(0, attempts2);
            Assert.Equal(1, fakeFinalize.Calls);
        }
        finally
        {
            try { Directory.Delete(tmp, true); } catch { }
        }
    }
}
