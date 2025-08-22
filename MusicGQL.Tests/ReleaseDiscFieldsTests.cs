using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Tests;

public class ReleaseDiscFieldsTests
{
    [Fact]
    public void DiscCount_Returns1_WhenNoDiscs()
    {
        // Arrange
        var json = new JsonRelease
        {
            Title = "Single Disc",
            ArtistName = "Test Artist",
            Type = JsonReleaseType.Album,
            Tracks =
            [
                new JsonTrack { Title = "T1", TrackNumber = 1 },
                new JsonTrack { Title = "T2", TrackNumber = 2 },
            ]
        };
        var cached = new CachedRelease
        {
            Title = json.Title,
            ArtistId = "Artist1",
            ArtistName = json.ArtistName,
            FolderName = "Release1",
            Type = json.Type,
            JsonRelease = json,
        };
        var release = new Release(cached);

        // Act
        var discCount = release.DiscCount();
        var discs = release.Discs().ToList();

        // Assert
        Assert.Equal(1, discCount);
        Assert.Single(discs);
        Assert.Equal(1, discs[0].DiscNumber());
        Assert.Null(discs[0].Title());
    }

    [Fact]
    public void DiscCount_And_Discs_ReturnsFromJson_WhenDiscsPresent()
    {
        // Arrange
        var json = new JsonRelease
        {
            Title = "Multi Disc",
            ArtistName = "Test Artist",
            Type = JsonReleaseType.Album,
            Discs =
            [
                new JsonDisc
                {
                    DiscNumber = 1,
                    Title = "Disc One",
                    Tracks = [ new JsonTrack { Title = "A", TrackNumber = 1 } ]
                },
                new JsonDisc
                {
                    DiscNumber = 2,
                    Title = "Disc Two",
                    Tracks = [ new JsonTrack { Title = "B", TrackNumber = 1 } ]
                },
            ]
        };
        var cached = new CachedRelease
        {
            Title = json.Title,
            ArtistId = "Artist1",
            ArtistName = json.ArtistName,
            FolderName = "Release1",
            Type = json.Type,
            JsonRelease = json,
        };
        var release = new Release(cached);

        // Act
        var discCount = release.DiscCount();
        var discs = release.Discs().ToList();

        // Assert
        Assert.Equal(2, discCount);
        Assert.Equal(2, discs.Count);
        Assert.Equal(1, discs[0].DiscNumber());
        Assert.Equal("Disc One", discs[0].Title());
        Assert.Equal(2, discs[1].DiscNumber());
        Assert.Equal("Disc Two", discs[1].Title());
    }
}

