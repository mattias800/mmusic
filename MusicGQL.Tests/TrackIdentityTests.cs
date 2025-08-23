using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;

namespace MusicGQL.Tests;

public class TrackIdentityTests
{
    [Fact]
    public void Track_Id_Includes_Disc_Number()
    {
        var ct = new CachedTrack
        {
            ArtistId = "Artist1",
            ArtistName = "Test Artist",
            ReleaseFolderName = "Release1",
            ReleaseTitle = "Test Release",
            DiscNumber = 2,
            TrackNumber = 1,
            JsonTrack = new JsonTrack { TrackNumber = 1, Title = "Song" },
        };

        var gqlTrack = new Track(ct);
        Assert.Equal("Artist1/Release1/2:1", gqlTrack.Id());
    }

    [Fact]
    public void LibraryAssetUrlFactory_Disc_Aware_Url()
    {
        var url = LibraryAssetUrlFactory.CreateTrackAudioUrl("A", "R", 2, 7);
        Assert.Equal("/library/A/releases/R/discs/2/tracks/7/audio", url);
    }
}
