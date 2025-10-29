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

    [Fact]
    public void TitleMatches_SingleWord_ShouldReject_SimplePluralization()
    {
        var title = "Grant Geissman - Flying Colors FLAC";
        var artist = "Grant";
        var album = "Color";
        var ok = ProwlarrTextMatch.TitleMatches(title, artist, album);
        Assert.False(ok);
    }

    [Fact]
    public void TitleMatches_SingleWord_Allows_SmallTypos()
    {
        var title = "Grant - Colr [FLAC]";
        var artist = "Grant";
        var album = "Color";
        var ok = ProwlarrTextMatch.TitleMatches(title, artist, album);
        Assert.True(ok);
    }

    [Theory]
    [InlineData("Artist-Album-2024-WEB-320-XYZ", "Artist", "Album", true)]
    [InlineData("Artist-Album-(EP)-2021-WEB-320-ABC", "Artist", "Album", true)]
    [InlineData("Artist-The_Album-2018-CD-320-Group", "Artist", "The Album", true)]
    [InlineData("Artist-Album_Remastered-2015-WEB-320-SCENE", "Artist", "Album", true)]
    [InlineData("DifferentArtist-Album-2020-WEB-320-GRP", "Artist", "Album", false)]
    [InlineData("Artist-DifferentAlbum-2019-WEB-320-GRP", "Artist", "Album", false)]
    [InlineData("Björk-Debut-1993", "Bjork", "Debut", true)]
    [InlineData("Tiësto-Kaleidoscope-2009", "Tiesto", "Kaleidoscope", true)]
    [InlineData("Sigur Rós-Með suð í eyrum við spilum endalaust-2008", "Sigur Ros", "Med sud i eyrum vid spilum endalaust", true)]
    [InlineData("Beyoncé-Lemonade-2016", "Beyonce", "Lemonade", true)]
    public void TitleMatches_SceneFolders(string folder, string artist, string album, bool expected)
    {
        // Simulate scene-style folder names (mp3 releases)
        var ok = ProwlarrTextMatch.TitleMatches(folder, artist, album);
        Assert.Equal(expected, ok);
    }
}

