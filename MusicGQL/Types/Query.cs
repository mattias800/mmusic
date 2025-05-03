using MusicGQL.Features.Artist;
using MusicGQL.Features.Recording;
using MusicGQL.Features.Release;
using MusicGQL.Features.User;

namespace MusicGQL.Types;

public class Query
{
    public ArtistSearchRoot Artist => new();
    public RecordingSearchRoot Recording => new();
    public ReleaseSearchRoot Release => new();
    public User Viewer => new(0);
}