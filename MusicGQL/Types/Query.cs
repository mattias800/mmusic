using MusicGQL.Features.Artist;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.External;
using MusicGQL.Features.Recording;
using MusicGQL.Features.Release;
using MusicGQL.Features.ReleaseGroups;
using MusicGQL.Features.User;

namespace MusicGQL.Types;

public class Query
{
    public ArtistSearchRoot Artist => new();
    public RecordingSearchRoot Recording => new();
    public ReleaseSearchRoot Release => new();
    public ReleaseGroupSearchRoot ReleaseGroup => new();
    public User Viewer => new(0);
    public DownloadsSearchRoot Download => new();
    public ExternalRoot External => new();
}
