namespace MusicGQL.Features.Release;

public record Track([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Track Model)
{
    [ID] public string Id => Model.Id;
    public int Position => Model.Position;
    public Recording.Recording Recording => new(Model.Recording);
};