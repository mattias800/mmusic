using Hqub.MusicBrainz;
using MusicGQL.Features.Release;

namespace MusicGQL.Features.Recording;

public record Recording([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Recording Model)
{
    [ID] public string Id => Model.Id;
    public string Title => Model.Title;
    public int? Length => Model.Length;

    public async Task<IEnumerable<Release.Release>> Releases([Service] MusicBrainzClient client)
    {
        var releases =
            await client.Releases.BrowseAsync("recording", Model.Id, 25, 0, "recordings", "genres", "release-groups");
        return releases.Select(a => new Release.Release(a));
    }

    public async Task<Release.Release?> MainAlbum([Service] MusicBrainzClient client)
    {
        var releases =
            await client.Releases.BrowseAsync("recording", Model.Id, 25, 0, "recordings", "genres", "release-groups");
        var mainAlbum = MainAlbumFinder.FindMainAlbum(releases);
        return mainAlbum is null ? null : new Release.Release(mainAlbum);
    }

    public async Task<IEnumerable<Artist.Artist>> Artists([Service] MusicBrainzClient client)
    {
        var artists = await client.Artists.BrowseAsync("recording", Model.Id);
        return artists.Select(a => new Artist.Artist(a));
    }
}