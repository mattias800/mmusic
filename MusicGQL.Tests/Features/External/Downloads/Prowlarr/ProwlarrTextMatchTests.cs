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
}

