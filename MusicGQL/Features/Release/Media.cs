namespace MusicGQL.Features.Release;

public record Medium([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Medium Model)
{
    public int TrackCount => Model.TrackCount;
    public IEnumerable<Track> Tracks => Model.Tracks.Select(t => new Track(t));
}