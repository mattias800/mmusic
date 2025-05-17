using Hqub.MusicBrainz.Entities;
using MusicGQL.Features.Artist;
using MusicGQL.Features.Release;
using MusicGQL.Integration.MusicBrainz;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.ReleaseGroups;

public record ReleaseGroup([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.ReleaseGroup Model)
{
    [ID]
    public string Id => Model.Id;
    public string Title => Model.Title;
    public string? PrimaryType => Model.PrimaryType;
    public IEnumerable<string> SecondaryTypes => Model.SecondaryTypes;

    public IEnumerable<NameCredit.NameCredit> Credits() =>
        Model.Credits?.Select(c => new NameCredit.NameCredit(c)) ?? [];

    public string? FirstReleaseDate => Model.FirstReleaseDate;
    public string? FirstReleaseYear => Model.FirstReleaseDate?.Split("-").FirstOrDefault();
    public IEnumerable<string> Tags => Model.Tags.Select(t => t.Name);

    public async Task<string?> CoverArtUri(
        [Service] IFanArtTVClient fanartClient,
        [Service] MusicBrainzService mbService
    )
    {
        var artistInfo = await fanartClient.Music.GetAlbumAsync(Model.Credits.First().Artist.Id);

        var m = artistInfo.Albums.GetValueOrDefault(Guid.Parse(Model.Id));

        if (m is not null)
        {
            var coverArt = m.AlbumCover?.FirstOrDefault()?.Url;
            if (coverArt is not null)
            {
                return coverArt;
            }
        }

        var all = await mbService.GetReleasesForReleaseGroupAsync(Id);
        var best = MainAlbumFinder.GetMainReleaseInReleaseGroup(all.ToList());

        return best is null ? null : CoverArtArchive.GetCoverArtUri(best.Id).ToString();
    }

    public async Task<AlbumImages?> Images([Service] IFanArtTVClient fanartClient)
    {
        try
        {
            var artistInfo = await fanartClient.Music.GetAlbumAsync(
                Model.Credits.First().Artist.Id
            );

            var m = artistInfo.Albums.GetValueOrDefault(Guid.Parse(Model.Id));
            return m is null ? null : new(m);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Release.Release?> MainRelease([Service] MusicBrainzService mbService)
    {
        var all = await mbService.GetReleasesForReleaseGroupAsync(Id);
        var best = MainAlbumFinder.GetMainReleaseInReleaseGroup(all.ToList());
        return best is null ? null : new Release.Release(best);
    }
}
