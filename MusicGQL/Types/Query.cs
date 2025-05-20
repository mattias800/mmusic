using MusicGQL.Features.Artist;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.External;
using MusicGQL.Features.Playlists;
using MusicGQL.Features.Recording;
using MusicGQL.Features.Release;
using MusicGQL.Features.ReleaseGroups;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.Users;

namespace MusicGQL.Types;

public class Query
{
    public ArtistSearchRoot Artist => new();
    public RecordingSearchRoot Recording => new();
    public ReleaseSearchRoot Release => new();
    public ReleaseGroupSearchRoot ReleaseGroup => new();
    // TODO: Implement proper Viewer field resolution based on authenticated user.
    // public User Viewer => new(0); // This is now problematic as User constructor expects UserProjection.
    public DownloadsSearchRoot Download => new();
    public ServerLibrarySearchRoot ServerLibrary => new();
    public ExternalRoot External => new();
    public PlaylistSearchRoot Playlist => new();
    public UserSearchRoot User => new();
}
