using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrTextMatchTests
{
    [Theory]
    [InlineData("Zara Larsson - Introducing", "Zara Larsson", "Introduction", true)]
    [InlineData("Zara Larsson - Introduction", "Zara Larsson", "Introduction", true)]
    [InlineData("Zara Larsson - Introdution", "Zara Larsson", "Introduction", true)]
    [InlineData("Zara Larsson - Intrduction", "Zara Larsson", "Introduction", true)]
    [InlineData("Zara Larsson - Introduction (Deluxe)", "Zara Larsson", "Introduction", true)]
    [InlineData("Zara - Introduction", "Zara Larsson", "Introduction", true)]
    [InlineData("Other Artist - Introduction", "Zara Larsson", "Introduction", false)]
    public void TitleMatches_Fuzzy(string title, string artist, string album, bool expected)
    {
        var ok = ProwlarrTextMatch.TitleMatches(title, artist, album);
        Assert.Equal(expected, ok);
    }

    [Fact]
    public void TitleMatches_ShouldReject_CompletelyDifferentAlbum()
    {
        var title = "Tom Grennan - Everywhere I Went Led Me To Where I Didnt Want To Be (2025) 24bit 48khz [FLAC]";
        var artist = "Breathe Carolina";
        var album = "Hello Fascination";
        var ok = ProwlarrTextMatch.TitleMatches(title, artist, album);
        Assert.False(ok);
    }

    [Fact]
    public void TitleMatches_ShouldNotMatch_ShortTokenOverlap()
    {
        // title contains short tokens like 'be', 'to', 'me' that should not fuzzy-match 'breathe' or 'carolina'
        var title = "Be To Me - Random Compilation FLAC";
        var artist = "Breathe Carolina";
        var album = "Hello Fascination";
        var ok = ProwlarrTextMatch.TitleMatches(title, artist, album);
        Assert.False(ok);
    }
}

