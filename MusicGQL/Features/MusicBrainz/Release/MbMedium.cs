namespace MusicGQL.Features.MusicBrainz.Release;

public record MbMedium([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Medium Model)
{
    public int TrackCount => Model.TrackCount;
    public IEnumerable<MbTrack> Tracks => Model.Tracks.Select(t => new MbTrack(t));
}
