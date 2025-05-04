using Hqub.MusicBrainz.Entities;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Release;

public record Release([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Release Model)
{
    [ID] public string Id => Model.Id;
    public string Title => Model.Title;
    public string? Date => Model.Date;
    public string? Year => Model.Date?.Split("-").FirstOrDefault();
    public string? Barcode => Model.Barcode;
    public string? Country => Model.Country;
    public string? Status => Model.Status;
    public string? Quality => Model.Quality;
    public IEnumerable<Genre> Genres => Model.Genres?.Select(g => new Genre(g)) ?? [];
    public IEnumerable<Medium> Media => Model.Media?.Select(m => new Medium(m)) ?? [];

    public ReleaseGroup? ReleaseGroup =>
        Model.ReleaseGroup is null ? null : new(Model.ReleaseGroup);

    public string CoverArtUri => CoverArtArchive.GetCoverArtUri(Model.Id).ToString();

    public async Task<IEnumerable<Artist.Artist>> Artists() =>
        Model.Credits.Select(a => new Artist.Artist(a.Artist));

    public async Task<IEnumerable<Recording.Recording>> Recordings(
        [Service] MusicBrainzService mbService
    )
    {
        var recordings = await mbService.GetRecordingsForReleaseAsync(Id);
        return recordings.Select(r => new Recording.Recording(r));
    }
}