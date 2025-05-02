using MusicGQL.Features.Artist;
using MusicGQL.Features.Recording;
using MusicGQL.Features.Release;

namespace MusicGQL.Types;

[QueryType]
public static class Query
{
    public static ArtistSearchRoot Artist => new();
    public static RecordingSearchRoot Recording => new();
    public static ReleaseSearchRoot Release => new();
}