using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrSearchStrategyTests
{
    [Fact]
    public void BuildQueries_BroadFirst_Order_NoYear()
    {
        var q = ProwlarrSearchStrategy.BuildQueries("Zara Larsson", "Introducing", null, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).ToList();
        Assert.Equal(new[]
        {
            "Zara Larsson Introducing",
            "Zara Larsson Introducing 320",
            "Zara Larsson Introducing FLAC"
        }, q);
    }

    [Fact]
    public void BuildQueries_BroadFirst_Order_WithYear()
    {
        var q = ProwlarrSearchStrategy.BuildQueries("Zara Larsson", "Introducing", 2013, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance).ToList();
        Assert.Equal(new[]
        {
            "Zara Larsson Introducing",
            "Zara Larsson Introducing 2013",
            "Zara Larsson Introducing 320",
            "Zara Larsson Introducing FLAC"
        }, q);
    }
}

