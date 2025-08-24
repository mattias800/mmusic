using MusicGQL.Features.External.Downloads.Prowlarr;

using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.External.Downloads.QBittorrent;
using MusicGQL.Features.External.Downloads.QBittorrent.Configuration;
using MusicGQL.Features.External.Downloads.Sabnzbd;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Reader;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Features.ServerSettings;
using HotChocolate.Subscriptions;
using HotChocolate.Subscriptions.InMemory;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrDownloadProviderPreferenceTests
{
    [Fact]
    public void Prefer_NZB_First_When_Both_NZB_And_Torrent_Exist()
    {
        // Arrange: results list contains a matching NZB-like (non-torrent downloadUrl) and a torrent-like candidate.
        var artist = "Artist";
        var album = "Album";
var nzb = new ProwlarrRelease("Artist - Album (FLAC)", null, null, "http://x/prowlarr/download/123", 1234, 1);
        var tor = new ProwlarrRelease("Artist - Album (WEB 1080p)", null, null, "http://x/prowlarr/download/123.torrent", 2234, 1);

        var list = new List<ProwlarrRelease> { tor, nzb };

        // Act: selection logic should choose NZB when SAB is allowed.
        var sel = ProwlarrSelectionLogic.Decide(list, artist, album, allowSab: true, allowQbit: true, discographyEnabled: false);

        // Assert
        Assert.Equal(ProwlarrSelectionType.Nzb, sel.Type);
        Assert.Equal(nzb.DownloadUrl, sel.UrlOrMagnet);
    }

    [Fact]
    public void Fall_Back_To_Torrent_When_NZB_Upload_Fails_Is_Handled_In_Provider()
    {
        // This behavior is integration in ProwlarrDownloadProvider; here we only assert selection still picks NZB first.
        var artist = "Artist";
        var album = "Album";
var nzb = new ProwlarrRelease("Artist - Album (MP3 320)", null, null, "http://x/dl/ok", 100, 1);
        var magnet = new ProwlarrRelease("Artist - Album [Magnet]", null, "magnet:?xt=urn:btih:abc", null, 90, 1);
        var list = new List<ProwlarrRelease> { magnet, nzb };
        var sel = ProwlarrSelectionLogic.Decide(list, artist, album, allowSab: true, allowQbit: true, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.Nzb, sel.Type);
    }

    [Fact]
    public async Task Provider_Falls_Back_To_Torrent_When_Nzb_Fetch_Or_Upload_Fails()
    {
        // Arrange: fake Prowlarr client returns both NZB and magnet; NZB URL will fail to fetch (connection error),
        // provider should fall back to qBittorrent magnet handoff.
        var artistId = "Artist";
        var releaseFolder = "Album";
        var artistName = "Artist";
        var releaseTitle = "Album";

        // Fake Prowlarr client
        var fakeProwlarr = new FakeProwlarrClient(new List<ProwlarrRelease>
        {
            new("Artist - Album (FLAC)", null, null, "http://127.0.0.1:1/nzb", 1234, 1), // will throw on fetch
            new("Artist - Album [Magnet]", null, "magnet:?xt=urn:btih:abc", null, 999, 1)
        });

        // In-memory server settings with SAB and qBit enabled, and a temp LogsFolderPath so logger doesn't throw
        var dbFactory = FakeDbFactory(seed: new DbServerSettings
        {
            Id = DefaultDbServerSettingsProvider.ServerSettingsSingletonId,
            EnableSabnzbdDownloader = true,
            EnableQBittorrentDownloader = true,
            DiscographyEnabled = false,
            LogsFolderPath = Path.GetTempPath()
        });
        var settingsAccessor = new ServerSettingsAccessor(dbFactory);
        var logProvider = new DownloadLogPathProvider(settingsAccessor);

        // qBittorrent HTTP handler to simulate login and add endpoints
        bool qbitAddCalled = false;
        var qbitHandler = new TestHttpHandler(async (req, ct) =>
        {
            var path = req.RequestUri?.AbsolutePath ?? string.Empty;
            if (path.EndsWith("/api/v2/auth/login"))
            {
                var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                resp.Headers.Add("Set-Cookie", "SID=abc");
                return resp;
            }
            if (path.EndsWith("/api/v2/torrents/add"))
            {
                qbitAddCalled = true;
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        });
        var qbitHttp = new HttpClient(qbitHandler);
        var qbitOptions = Options.Create(new QBittorrentOptions
        {
            BaseUrl = "http://qb.local",
            Username = "u",
            Password = "p",
            SavePath = null
        });
        var qb = new QBittorrentClient(qbitHttp, qbitOptions, NullLogger<QBittorrentClient>.Instance, logProvider);

        // SAB options deliberately invalid so content upload would fail if reached
        var sabOptions = Options.Create(new SabnzbdOptions { BaseUrl = null, ApiKey = null, Category = "music" });
        var sab = new SabnzbdClient(new HttpClient(new TestHttpHandler((req, ct) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)))) , sabOptions, NullLogger<SabnzbdClient>.Instance, logProvider);

        // Provider options
        var prowOptions = Options.Create(new MusicGQL.Features.External.Downloads.Prowlarr.Configuration.ProwlarrOptions
        {
            BaseUrl = "http://prowl.local",
            ApiKey = "k",
            IndexerIds = null,
            TimeoutSeconds = 10,
            MaxRetries = 0,
            RetryDelaySeconds = 0,
            TestConnectivityFirst = false,
            EnableDetailedLogging = false
        });

        // library cache with dummy event sender
        var cache = new ServerLibraryCache(new ServerLibraryJsonReader(settingsAccessor), new DummyEventSender());

        var provider = new MusicGQL.Features.External.Downloads.Prowlarr.ProwlarrDownloadProvider(
            fakeProwlarr,
            qb,
            sab,
            prowOptions,
            sabOptions,
            settingsAccessor,
            NullLogger<MusicGQL.Features.External.Downloads.Prowlarr.ProwlarrDownloadProvider>.Instance,
            logProvider,
            cache
        );

        // Act
        var ok = await provider.TryDownloadReleaseAsync(
            artistId,
            releaseFolder,
            artistName,
            releaseTitle,
            targetDirectory: "/tmp",
            allowedOfficialCounts: new List<int> { 5 },
            allowedOfficialDigitalCounts: new List<int> { 5 },
            cancellationToken: CancellationToken.None
        );

        // Assert: provider should have fallen back to qBittorrent magnet handoff
        Assert.True(ok, "Provider did not succeed via torrent fallback");
        Assert.True(qbitAddCalled, "qBittorrent add endpoint was not called in fallback");
    }

    private static Microsoft.EntityFrameworkCore.IDbContextFactory<EventDbContext> FakeDbFactory(DbServerSettings? seed = null)
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var factory = new PooledDbContextFactory<EventDbContext>(options);
        using var db = factory.CreateDbContext();
        if (seed != null)
        {
            db.ServerSettings.Add(seed);
            db.SaveChanges();
        }
        else
        {
            db.ServerSettings.Add(DefaultDbServerSettingsProvider.GetDefault());
            db.SaveChanges();
        }
        return factory;
    }

    private sealed class FakeProwlarrClient : IProwlarrClient
    {
        private readonly IReadOnlyList<ProwlarrRelease> _results;
        public FakeProwlarrClient(IReadOnlyList<ProwlarrRelease> results) => _results = results;
        public Task<IReadOnlyList<ProwlarrRelease>> SearchAlbumAsync(string artistName, string releaseTitle, int? year, CancellationToken cancellationToken, IDownloadLogger? relLogger = null)
            => Task.FromResult(_results);
        public Task<(bool ok, string message)> TestConnectivityAsyncPublic(CancellationToken cancellationToken) => Task.FromResult((true, "OK"));
        public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
        public string GetClientInfo() => "FakeProwlarr";
    }

    private sealed class TestHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;
        public TestHttpHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }

    private sealed class DummyEventSender : ITopicEventSender
    {
        public ValueTask SendAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public ValueTask CompleteAsync(string topic)
            => ValueTask.CompletedTask;
    }
}

