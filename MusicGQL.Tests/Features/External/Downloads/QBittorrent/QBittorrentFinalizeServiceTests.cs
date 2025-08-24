using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.External.Downloads.QBittorrent;
using MusicGQL.Features.External.Downloads.QBittorrent.Configuration;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Reader;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Features.ServerSettings.Db;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MusicGQL.Tests.Features.External.Downloads.QBittorrent;

public class QBittorrentFinalizeServiceTests
{
    private sealed class TestHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;
        public TestHttpHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }

    private sealed class DummyEventSender : HotChocolate.Subscriptions.ITopicEventSender
    {
        public ValueTask SendAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask CompleteAsync(string topic) => ValueTask.CompletedTask;
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

    private static void CreateLibrary(string root, string artistId, string releaseFolder, string artistName, string releaseTitle)
    {
        var artistDir = Path.Combine(root, artistId);
        Directory.CreateDirectory(artistDir);
        var releaseDir = Path.Combine(artistDir, releaseFolder);
        Directory.CreateDirectory(releaseDir);
        File.WriteAllText(Path.Combine(artistDir, "artist.json"), "{\"id\":\"" + artistId + "\",\"name\":\"" + artistName + "\"}");
        File.WriteAllText(Path.Combine(releaseDir, "release.json"), "{\"title\":\"" + releaseTitle + "\",\"artistName\":\"" + artistName + "\",\"tracks\":[{\"title\":\"A\",\"trackNumber\":1},{\"title\":\"B\",\"trackNumber\":2}]}");
    }

    [Fact]
    public async Task Finalize_Skips_When_Torrent_Not_Complete()
    {
        // Arrange
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        try
        {
            var artistId = "Artist";
            var artistName = "Artist";
            var releaseFolder = "Album";
            var releaseTitle = "Album";
            CreateLibrary(tmp, artistId, releaseFolder, artistName, releaseTitle);

            var dbFactory = CreateDbFactoryWithSettings(tmp, Path.GetTempPath());
            var settings = new ServerSettingsAccessor(dbFactory);
            var cache = new ServerLibraryCache(new ServerLibraryJsonReader(settings), new DummyEventSender());

            // HTTP responder: login OK, torrents list with progress 0.7
            var http = new HttpClient(new TestHttpHandler(async (req, ct) =>
            {
                var path = req.RequestUri?.AbsolutePath ?? string.Empty;
                if (path.EndsWith("/api/v2/auth/login"))
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Headers.Add("Set-Cookie", "SID=abc");
                    return resp;
                }
                if (path.EndsWith("/api/v2/torrents/info"))
                {
                    var body = "[ { \"hash\": \"abc\", \"name\": \"" + artistName + " - " + releaseTitle + "\", \"progress\": 0.7, \"state\": \"downloading\", \"save_path\": null, \"content_path\": null } ]";
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(body)
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }));

            var qbOptions = Options.Create(new QBittorrentOptions
            {
                BaseUrl = "http://qb.local",
                Username = "u",
                Password = "p"
            });
            var logProvider = new DownloadLogPathProvider(settings);
            var qbClient = new QBittorrentClient(http, qbOptions, NullLogger<QBittorrentClient>.Instance, logProvider);
            var svc = new QBittorrentFinalizeService(qbClient, cache, settings, NullLogger<QBittorrentFinalizeService>.Instance, logProvider);

            // Act
            var ok = await svc.FinalizeReleaseAsync(artistId, releaseFolder, CancellationToken.None);

            // Assert
            Assert.False(ok);
        }
        finally
        {
            try { Directory.Delete(tmp, true); } catch { }
        }
    }
}
