using Hqub.MusicBrainz;

namespace MusicGQL.Features.Artist;

public record Artist([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Artist Model)
{
    [ID] public string Id => Model.Id;
    public string Name => Model.Name;
    public string SortName => Model.SortName;
    public string? Disambiguation => Model.Disambiguation;
    public string? Type => Model.Type;

    public async Task<IEnumerable<Release.Release>> Releases([Service] MusicBrainzClient client)
    {
        var releases = await client.Releases.BrowseAsync("artist", Id, 25, 0, "recordings", "genres", "release-groups");
        return releases.Items.Select(r => new Release.Release(r));
    }
}