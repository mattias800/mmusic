using Hqub.MusicBrainz;
using Hqub.MusicBrainz.Entities;

namespace MusicGQL.Features.Release;

public record Release([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Release Model)
{
    [ID] public string Id => Model.Id;
    public string Title => Model.Title;
    public string? Barcode => Model.Barcode;
    public IEnumerable<Genre> Genres => Model.Genres?.Select(g => new Genre(g)) ?? [];
    public IEnumerable<Medium> Media => Model.Media?.Select(m => new Medium(m)) ?? [];
    public ReleaseGroup? ReleaseGroup => Model.ReleaseGroup is null ? null : new(Model.ReleaseGroup);

    public string CoverArtUri => CoverArtArchive.GetCoverArtUri(Model.Id).ToString();

    public async Task<IEnumerable<Recording.Recording>> Recordings([Service] MusicBrainzClient client)
    {
        var recordings = await client.Recordings.BrowseAsync("release", Id);
        return recordings.Items.Select(r => new Recording.Recording(r));
    }
}