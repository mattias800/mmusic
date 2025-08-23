using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrQueryBuilderTests
{
    [Fact]
public void BuildCandidateUrls_Includes_Audio_Categories_And_Repeated_Indexers()
    {
        var urls = ProwlarrQueryBuilder.BuildCandidateUrls(
            baseUrl: "http://localhost:9696/",
            apiKey: "KEY",
            query: "Zara Larsson Introducing",
            indexerIds: new[] { 3, 12 },
            logger: null
        ).ToList();

        Assert.Equal(2, urls.Count);

        // First URL should be strongly scoped with categories and indexers
        var first = urls[0];
        Assert.Contains("query=Zara%20Larsson%20Introducing", first);
        Assert.Contains("categories=3000", first);
        Assert.Contains("categories=3010", first);
        Assert.Contains("categories=3040", first);
        Assert.Contains("categories=3050", first);
        Assert.Contains("indexers=3", first);
        Assert.Contains("indexers=12", first);

        // No legacy fallbacks
        Assert.DoesNotContain(urls, u => u.Contains("&q=", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(urls, u => u.Contains("&term=", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(urls, u => u.Contains("type=search", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
public void BuildCandidateUrls_Without_Indexers_Omits_Indexer_Params()
    {
        var urls = ProwlarrQueryBuilder.BuildCandidateUrls(
            baseUrl: "http://x:9696",
            apiKey: "k",
            query: "Artist Album",
            indexerIds: null,
            logger: null
        ).ToList();

        Assert.True(urls.Count > 0);
        Assert.All(urls, u => Assert.DoesNotContain("indexers=", u));
        // Should always include audio categories
        Assert.All(urls, u => Assert.Contains("categories=3000", u));
        Assert.All(urls, u => Assert.Contains("categories=3010", u));
        Assert.All(urls, u => Assert.Contains("categories=3040", u));
        Assert.All(urls, u => Assert.Contains("categories=3050", u));
    }
}

