using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.MusicBrainz.Recording;
using MusicGQL.Features.MusicBrainz.Release;
using MusicGQL.Features.MusicBrainz.ReleaseGroup;

namespace MusicGQL.Features.MusicBrainz;

public record MusicBrainzSearchRoot
{
    public MusicBrainzArtistSearchRoot Artist() => new();

    public MusicBrainzReleaseGroupSearchRoot ReleaseGroup() => new();

    public MusicBrainzReleaseSearchRoot Release() => new();

    public MusicBrainzRecordingSearchRoot Recording() => new();
};
