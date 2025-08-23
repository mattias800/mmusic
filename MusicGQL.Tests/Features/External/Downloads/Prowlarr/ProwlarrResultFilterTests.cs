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
}

