using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrResultFilterTests
{
    [Fact]
    public void IsValidMusicResult_Rejects_NonMusicTerms()
    {
        var rel = new ProwlarrRelease("Some Show S01E01 1080p", null, null, null, null, null);
        Assert.False(ProwlarrResultFilter.IsValidMusicResult(rel, "Radiohead", "OK Computer"));
    }

    [Fact]
    public void IsValidMusicResult_Accepts_BasicMatch()
    {
        var rel = new ProwlarrRelease("Radiohead - OK Computer FLAC", null, null, null, 123L, null);
        Assert.True(ProwlarrResultFilter.IsValidMusicResult(rel, "Radiohead", "OK Computer"));
    }

    [Fact]
    public void GetRejectionReason_Reports_TitleMismatch()
    {
        var rel = new ProwlarrRelease("Some Other Artist - Some Album", null, null, null, null, null);
        var reason = ProwlarrResultFilter.GetRejectionReason(rel, "Radiohead", "OK Computer");
        Assert.Contains("Artist", reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsValidMusicResult_SingleWord_Rejects_SimplePluralization()
    {
        var rel = new ProwlarrRelease("Grant Geissman - Flying Colors FLAC", null, null, "http://x/whatever.nzb", 306_794_000L, 1);
        var ok = ProwlarrResultFilter.IsValidMusicResult(rel, "Grant", "Color");
        Assert.False(ok);
    }

    [Theory]
    [InlineData("Artist-Album-2024-WEB-320-XYZ", true)]
    [InlineData("Artist-Album-(EP)-2021-WEB-320-ABC", true)]
    [InlineData("Artist-The_Album-2018-CD-320-Group", true)]
    [InlineData("DifferentArtist-Album-2020-WEB-320-GRP", false)]
    [InlineData("Artist-DifferentAlbum-2019-WEB-320-GRP", false)]
    [InlineData("Björk-Debut-1993", true)]
    [InlineData("Tiësto-Kaleidoscope-2009", true)]
    [InlineData("Sigur Rós-Með suð í eyrum við spilum endalaust-2008", true)]
    [InlineData("Beyoncé-Lemonade-2016", true)]
    public void IsValidMusicResult_SceneFolderPatterns(string folder, bool expected)
    {
        var rel = new ProwlarrRelease(folder, null, null, "http://x/file.nzb", 100_000_000L, 1);
        string artist = "Artist";
        string album = folder.Contains("The_Album") ? "The Album" : "Album";
        if (folder.Contains("Björk")) { artist = "Bjork"; album = "Debut"; }
        if (folder.Contains("Tiësto")) { artist = "Tiesto"; album = "Kaleidoscope"; }
        if (folder.Contains("Sigur Rós")) { artist = "Sigur Ros"; album = "Med sud i eyrum vid spilum endalaust"; }
        if (folder.Contains("Beyoncé")) { artist = "Beyonce"; album = "Lemonade"; }
        var ok = ProwlarrResultFilter.IsValidMusicResult(rel, artist, album);
        Assert.Equal(expected, ok);
    }
}

