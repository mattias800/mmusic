using MusicGQL.Features.Downloads.Util;

namespace MusicGQL.Tests;

public class FileNameParsingTests
{
    [Theory]
    [InlineData("Artist - Album - 01 - Title.mp3", 1, 1)]
    [InlineData("CD2/02 - Song.flac", 2, 2)]
    [InlineData("Disc 3 - 07 - Track.m4a", 3, 7)]
    [InlineData("digital media 10 somefile 12 name.mp3", 10, 12)]
    [InlineData("11 - Something.mp3", 1, 11)]
    [InlineData("Track 1103 Name.mp3", 1, 3)]
    public void ExtractDiscTrackFromName_Works(string name, int expDisc, int expTrack)
    {
        var (disc, track) = FileNameParsing.ExtractDiscTrackFromName(name);
        Assert.Equal(expDisc, disc);
        Assert.Equal(expTrack, track);
    }
}
