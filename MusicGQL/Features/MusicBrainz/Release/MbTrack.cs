namespace MusicGQL.Features.MusicBrainz.Release;

public record MbTrack([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Track Model)
{
    [ID]
    public string Id => Model.Id;
    public int Position => Model.Position;
    public Recording.MbRecording MbRecording => new(Model.Recording);
};
