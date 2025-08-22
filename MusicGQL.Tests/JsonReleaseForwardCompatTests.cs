using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Tests;

public class JsonReleaseForwardCompatTests
{
    private static JsonSerializerOptions Options => new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    [Fact]
    public void Deserialize_DiscsOnly_DeserializesOK()
    {
        var json = @"{
  ""title"": ""Test Multi-Disc"",
  ""artistName"": ""Test Artist"",
  ""type"": ""album"",
  ""discs"": [
    {
      ""discNumber"": 1,
      ""title"": ""Disc One"",
      ""tracks"": [
        { ""title"": ""One-A"", ""trackNumber"": 1 },
        { ""title"": ""One-B"", ""trackNumber"": 2 }
      ]
    },
    {
      ""discNumber"": 2,
      ""title"": ""Disc Two"",
      ""tracks"": [
        { ""title"": ""Two-A"", ""trackNumber"": 1 },
        { ""title"": ""Two-B"", ""trackNumber"": 2 }
      ]
    }
  ]
}";

        var release = JsonSerializer.Deserialize<JsonRelease>(json, Options);
        Assert.NotNull(release);
        Assert.NotNull(release!.Discs);
        Assert.Equal(2, release.Discs!.Count);
        Assert.Null(release.Tracks); // No flattened tracks provided
        Assert.Equal(1, release.Discs[0].DiscNumber);
        Assert.Equal("Disc One", release.Discs[0].Title);
        Assert.Equal("One-A", release.Discs[0].Tracks[0].Title);
        Assert.Equal(1, release.Discs[0].Tracks[0].TrackNumber);
        Assert.Null(release.Discs[0].Tracks[0].DiscNumber); // For raw deserialization, this can be null; builder populates it when constructing
    }

    [Fact]
    public void Deserialize_DiscsPlusFlattenedTracks_RespectsPrecedence()
    {
        var json = @"{
  ""title"": ""Test With Both"",
  ""artistName"": ""Test Artist"",
  ""type"": ""album"",
  ""discs"": [
    {
      ""discNumber"": 1,
      ""tracks"": [ { ""title"": ""A"", ""trackNumber"": 1 } ]
    }
  ],
  ""tracks"": [
    { ""title"": ""A"", ""trackNumber"": 1, ""discNumber"": 1 }
  ]
}";

        var release = JsonSerializer.Deserialize<JsonRelease>(json, Options);
        Assert.NotNull(release);
        // At the JSON model level both can be present; precedence is enforced when loading cache
        Assert.NotNull(release!.Discs);
        Assert.NotNull(release.Tracks);
        Assert.Single(release.Discs![0].Tracks);
        Assert.Single(release.Tracks!);
        Assert.Equal(1, release.Tracks![0].DiscNumber);
    }
}

