using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.External.Downloads.QBittorrent;
using MusicGQL.Features.External.Downloads.QBittorrent.Configuration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MusicGQL.Tests.Features.External.Downloads.QBittorrent;

public class QBittorrentClientTests
{
    private sealed class TestHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;
        public TestHttpHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }

    private sealed class TestDbFactory : Microsoft.EntityFrameworkCore.IDbContextFactory<MusicGQL.Db.Postgres.EventDbContext>
    {
        private readonly DbContextOptions<MusicGQL.Db.Postgres.EventDbContext> _options;
        public TestDbFactory(DbContextOptions<MusicGQL.Db.Postgres.EventDbContext> options) => _options = options;
        public MusicGQL.Db.Postgres.EventDbContext CreateDbContext() => new MusicGQL.Db.Postgres.EventDbContext(_options);
    }

    private static QBittorrentClient CreateClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
    {
        var http = new HttpClient(new TestHttpHandler(responder));
        var opts = Options.Create(new QBittorrentOptions
        {
            BaseUrl = "http://qb.local",
            Username = "user",
            Password = "pass",
            SavePath = null,
        });
        var logs = Options.Create(new MusicGQL.Features.ServerSettings.Db.DbServerSettings
        {
            Id = MusicGQL.Features.ServerSettings.Db.DefaultDbServerSettingsProvider.ServerSettingsSingletonId,
            LogsFolderPath = Path.GetTempPath(),
        });
        // Minimal server settings accessor to satisfy DownloadLogPathProvider
        var optionsBuilder = new DbContextOptionsBuilder<MusicGQL.Db.Postgres.EventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());
        var dbFactory = new TestDbFactory(optionsBuilder.Options);
        using (var db = dbFactory.CreateDbContext())
        {
            db.ServerSettings.Add(new MusicGQL.Features.ServerSettings.Db.DbServerSettings
            {
                Id = MusicGQL.Features.ServerSettings.Db.DefaultDbServerSettingsProvider.ServerSettingsSingletonId,
                LogsFolderPath = Path.GetTempPath(),
            });
            db.SaveChanges();
        }
        var accessor = new MusicGQL.Features.ServerSettings.ServerSettingsAccessor(dbFactory);
        var logPathProvider = new DownloadLogPathProvider(accessor);
        return new QBittorrentClient(http, opts, NullLogger<QBittorrentClient>.Instance, logPathProvider);
    }

    [Fact]
    public async Task AddMagnet_ReturnsTrue_WhenBodyIsOk()
    {
        var client = CreateClient(async (req, ct) =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/api/v2/auth/login"))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Headers.Add("Set-Cookie", "SID=abc");
                return resp;
            }
            if (req.RequestUri!.AbsolutePath.EndsWith("/api/v2/torrents/add"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Ok.")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var ok = await client.AddMagnetAsync("magnet:?xt=urn:btih:abc", null, CancellationToken.None);
        Assert.True(ok);
    }

    [Fact]
    public async Task AddMagnet_ReturnsFalse_WhenBodyIsNotOk()
    {
        var client = CreateClient(async (req, ct) =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/api/v2/auth/login"))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Headers.Add("Set-Cookie", "SID=abc");
                return resp;
            }
            if (req.RequestUri!.AbsolutePath.EndsWith("/api/v2/torrents/add"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Fails.")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var ok = await client.AddMagnetAsync("magnet:?xt=urn:btih:abc", null, CancellationToken.None);
        Assert.False(ok);
    }

    [Fact]
    public async Task AddByUrl_ReturnsFalse_OnNonOkBody()
    {
        var client = CreateClient(async (req, ct) =>
        {
            if (req.RequestUri!.AbsolutePath.EndsWith("/api/v2/auth/login"))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Headers.Add("Set-Cookie", "SID=abc");
                return resp;
            }
            if (req.RequestUri!.AbsolutePath.EndsWith("/api/v2/torrents/add"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Error")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var ok = await client.AddByUrlAsync("http://x/abc.torrent", null, CancellationToken.None);
        Assert.False(ok);
    }
}

