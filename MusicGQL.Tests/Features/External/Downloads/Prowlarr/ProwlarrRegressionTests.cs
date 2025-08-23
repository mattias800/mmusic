using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using MusicGQL.Features.External.Downloads.Prowlarr;

namespace MusicGQL.Tests.Features.External.Downloads.Prowlarr;

public class ProwlarrRegressionTests
{
    [Fact]
    public void ParseResults_ShouldReject_TomGrennan_WhenSearching_BreatheCarolina_HelloFascination()
    {
        var json = """
[
  {
    "title": "AC/DC - Back In Black (1980) [FLAC] CD 431046",
    "downloadUrl": "http://x/1.nzb"
  },
  {
    "title": "Tom Grennan - Everywhere I Went Led Me To Where I Didnt Want To Be (2025) 24bit 48khz [FLAC]",
    "downloadUrl": "http://x/2.nzb"
  },
  {
    "title": "AC/DC - Fly On The Wall (1985)(7567-81263-2) FLAC",
    "downloadUrl": "http://x/3.nzb"
  }
]
""";
        using var doc = JsonDocument.Parse(json);
        var list = ProwlarrJsonParser.ParseResults(
            doc.RootElement,
            artistName: "Breathe Carolina",
            releaseTitle: "Hello Fascination",
            NullLogger.Instance);

        // Expect no results because none of the titles match artist+album
        Assert.Empty(list);
    }
}

