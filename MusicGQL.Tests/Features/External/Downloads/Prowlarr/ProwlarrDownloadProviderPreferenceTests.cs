using MusicGQL.Features.External.Downloads.Prowlarr;

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
}

