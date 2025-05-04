using MusicGQL.Features.Release;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Recording;

public record Recording([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Recording Model)
{
    [ID] public string Id => Model.Id;
    public string Title => Model.Title;
    public int? Length => Model.Length;

    public async Task<IEnumerable<Release.Release>> Releases([Service] MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForRecordingAsync(Model.Id);
        return releases.Select(a => new Release.Release(a));
    }

    public async Task<Release.Release> MainAlbum([Service] MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForRecordingAsync(Model.Id);
        var mainAlbum = MainAlbumFinder.FindMainAlbum(releases);
        return new Release.Release(mainAlbum);
    }

    public async Task<IEnumerable<Artist.Artist>> Artists([Service] MusicBrainzService mbService)
    {
        var artists = await mbService.GetArtistsForRecordingAsync(Model.Id);
        return artists.Select(a => new Artist.Artist(a));
    }
}