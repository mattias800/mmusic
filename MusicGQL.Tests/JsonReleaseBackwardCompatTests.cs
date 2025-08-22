using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Tests;

public class JsonReleaseBackwardCompatTests
{
    [Fact]
    public void Deserialize_TracksOnly_DeserializesOK()
    {
        // Arrange: a legacy/flat JSON shape with only tracks[] and no discs[]
        var json = @"{
  ""title"": ""Test Release"",
  ""artistName"": ""Test Artist"",
  ""type"": ""album"",
  ""tracks"": [
    { ""title"": ""Intro"", ""trackNumber"": 1 },
    { ""title"": ""Song A"", ""trackNumber"": 2 }
  ]
}";
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        // Act
        var release = JsonSerializer.Deserialize<JsonRelease>(json, options);

        // Assert
        Assert.NotNull(release);
        Assert.Equal("Test Release", release!.Title);
        Assert.Equal("Test Artist", release.ArtistName);
        Assert.Equal(JsonReleaseType.Album, release.Type);
        Assert.Null(release.Discs); // No discs provided
        Assert.NotNull(release.Tracks);
        Assert.Equal(2, release.Tracks!.Count);
        Assert.Equal("Intro", release.Tracks[0].Title);
        Assert.Equal(1, release.Tracks[0].TrackNumber);
        Assert.Null(release.Tracks[0].DiscNumber); // Not present in flat view
    }
}

