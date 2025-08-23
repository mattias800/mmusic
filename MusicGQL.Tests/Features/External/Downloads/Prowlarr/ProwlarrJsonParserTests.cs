using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrJsonParserTests
{
    [Fact]
    public void ParseResults_Handles_ArrayRoot()
    {
        var json = "[{\"title\":\"Artist - Album\",\"downloadUrl\":\"http://x/file.nzb\"}]";
        using var doc = JsonDocument.Parse(json);
        var list = ProwlarrJsonParser.ParseResults(doc.RootElement, "Artist", "Album", NullLogger.Instance);
        Assert.Single(list);
        Assert.Equal("Artist - Album", list[0].Title);
    }

    [Fact]
    public void ParseResults_Handles_ObjectResults()
    {
        var json = "{\"results\":[{\"title\":\"Artist - Album\",\"magnetUrl\":\"magnet:?xt=urn:btih:abc\"}]}";
        using var doc = JsonDocument.Parse(json);
        var list = ProwlarrJsonParser.ParseResults(doc.RootElement, "Artist", "Album", NullLogger.Instance);
        Assert.Single(list);
        Assert.Equal("Artist - Album", list[0].Title);
    }
}
