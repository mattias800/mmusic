using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrSelectionLogicTests
{
    [Fact]
    public void Decide_Prefers_Nzb_When_Sab_Allowed()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Artist - Album FLAC", null, null, "http://x/file.nzb", 1000, null),
            new("Artist - Album", null, "magnet:?xt=urn:btih:abc", null, 900, null)
        };

        var sel = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: true, allowQbit: true, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.Nzb, sel.Type);
        Assert.Equal("http://x/file.nzb", sel.UrlOrMagnet);
    }

    [Fact]
    public void Decide_Prefers_Magnet_When_No_Nzb_And_Qbit_Allowed()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Artist - Album", null, "magnet:?xt=urn:btih:abc", null, 900, null)
        };
        var sel = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: true, allowQbit: true, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.Magnet, sel.Type);
        Assert.StartsWith("magnet:", sel.UrlOrMagnet);
    }

    [Fact]
    public void Decide_Uses_Torrent_When_Qbit_Allowed()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Artist - Album", null, null, "http://x/file.torrent", 1200, null)
        };
        var sel = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: false, allowQbit: true, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.Torrent, sel.Type);
        Assert.EndsWith(".torrent", sel.UrlOrMagnet);
    }

    [Fact]
    public void Decide_Discography_Only_When_Enabled()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Artist - Complete Discography", null, null, "http://x/file.nzb", 1000000, null)
        };
        var selDisabled = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: true, allowQbit: true, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.None, selDisabled.Type);

        var selEnabled = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: true, allowQbit: true, discographyEnabled: true);
        Assert.Equal(ProwlarrSelectionType.Nzb, selEnabled.Type);
        Assert.True(selEnabled.IsDiscography);
    }

    [Fact]
    public void Decide_Prefers_Magnet_Over_Torrent_When_Both_Present()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Artist - Album [Torrent]", null, null, "http://x/file.torrent", 1200, null),
            new("Artist - Album [Magnet]", null, "magnet:?xt=urn:btih:abc", null, 1100, null)
        };
        var sel = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: false, allowQbit: true, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.Magnet, sel.Type);
        Assert.StartsWith("magnet:", sel.UrlOrMagnet);
    }

    [Fact]
    public void Decide_Returns_None_When_No_Matching_Title()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Random Movie 1080p", null, null, null, null, null),
            new("Another Show S01E01", null, null, null, null, null)
        };
        var sel = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: true, allowQbit: true, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.None, sel.Type);
        Assert.Null(sel.Release);
    }

    [Fact]
    public void Decide_Discography_Torrent_Preferred_Over_Magnet_When_Qbit_Allowed()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Artist - Complete Discography [Magnet]", null, "magnet:?xt=urn:btih:def", null, 2000, null),
            new("Artist - Complete Discography [Torrent]", null, null, "http://x/pack.torrent", 2500, null)
        };
        var sel = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: false, allowQbit: true, discographyEnabled: true);
        Assert.Equal(ProwlarrSelectionType.Torrent, sel.Type);
        Assert.True(sel.IsDiscography);
        Assert.EndsWith(".torrent", sel.UrlOrMagnet);
    }

    [Fact]
    public void Decide_Prefers_NonDiscography_When_Both_Nzb_Available_And_Sab_Allowed()
    {
        var normal = new ProwlarrRelease("Artist - Album", null, null, "http://x/album.nzb", 800, null);
        var discog = new ProwlarrRelease("Artist - Complete Discography", null, null, "http://x/pack.nzb", 999999, null);
        var results = new List<ProwlarrRelease> { discog, normal };
        var sel = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: true, allowQbit: false, discographyEnabled: true);
        Assert.Equal(ProwlarrSelectionType.Nzb, sel.Type);
        Assert.False(sel.IsDiscography);
        Assert.Equal("http://x/album.nzb", sel.UrlOrMagnet);
    }

    [Fact]
    public void Decide_None_When_Only_Torrent_And_Qbit_Disallowed()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Artist - Album", null, null, "http://x/file.torrent", 1200, null)
        };
        var sel = ProwlarrSelectionLogic.Decide(results, "Artist", "Album", allowSab: true, allowQbit: false, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.None, sel.Type);
    }

    [Fact]
    public void Decide_Fuzzy_Introduction_vs_Introducing_Prefers_Nzb()
    {
        var results = new List<ProwlarrRelease>
        {
            new("Zara Larsson - Introducing FLAC", null, null, "http://x/intro.nzb", 1500, null),
            new("Zara Larsson - Introduction [Magnet]", null, "magnet:?xt=urn:btih:123", null, 1200, null)
        };
        var sel = ProwlarrSelectionLogic.Decide(results, "Zara Larsson", "Introduction", allowSab: true, allowQbit: true, discographyEnabled: false);
        Assert.Equal(ProwlarrSelectionType.Nzb, sel.Type);
        Assert.Equal("http://x/intro.nzb", sel.UrlOrMagnet);
    }
}

