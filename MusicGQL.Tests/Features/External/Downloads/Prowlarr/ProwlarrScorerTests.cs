using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrScorerTests
{
    [Fact]
    public void IsTorrentFile_Detects_Torrent()
    {
        Assert.True(ProwlarrScorer.IsTorrentFile("http://example.com/file.torrent"));
        Assert.True(ProwlarrScorer.IsTorrentFile("http://example.com/download?type=torrent"));
        Assert.False(ProwlarrScorer.IsTorrentFile("http://example.com/file.nzb"));
    }

    [Fact]
    public void CalculateRelevanceScore_Prefers_ExactMatches()
    {
        var a = new ProwlarrRelease("Artist - Album FLAC", null, null, "http://example.com/file.nzb", 1000, null);
        var b = new ProwlarrRelease("Other - Something", null, null, "http://example.com/file.nzb", 1000, null);
        var sa = ProwlarrScorer.CalculateRelevanceScore(a, "Artist", "Album");
        var sb = ProwlarrScorer.CalculateRelevanceScore(b, "Artist", "Album");
        Assert.True(sa > sb);
    }
}

