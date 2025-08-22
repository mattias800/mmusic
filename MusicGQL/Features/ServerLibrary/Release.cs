using MusicGQL.Features.Artists;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary;

public enum ReleaseType
{
    Album,
    Ep,
    Single,
}

public record Release([property: GraphQLIgnore] CachedRelease Model)
{
    [ID]
    public string Id() => Model.ArtistId + "/" + Model.SearchFolderName;

    public string Title() => Model.Title;

    public string ArtistName() => Model.JsonRelease.ArtistName;

    public ReleaseType? Type() =>
        Model.Type switch
        {
            JsonReleaseType.Album => ReleaseType.Album,
            JsonReleaseType.Ep => ReleaseType.Ep,
            JsonReleaseType.Single => ReleaseType.Single,
            _ => null,
        };

    public string FolderName() => Model.FolderName;

    public async Task<Artist> Artist(ServerLibraryCache cache)
    {
        var artist = await cache.GetArtistByIdAsync(Model.ArtistId);

        if (artist is null)
        {
            throw new Exception("Could not find artist, id=" + Model.ArtistId);
        }

        return new(artist);
    }

    public string? FirstReleaseDate() => Model.JsonRelease.FirstReleaseDate;

    public string? FirstReleaseYear() => Model.JsonRelease.FirstReleaseYear;

    /// <summary>
    /// Gets the cover art URL that the server can serve
    /// </summary>
    public string CoverArtUrl() =>
        LibraryAssetUrlFactory.CreateReleaseCoverArtUrl(Model.ArtistId, Model.FolderName);

    public async Task<IEnumerable<Track>> Tracks(ServerLibraryCache cache)
    {
        var tracks = await cache.GetAllTracksForReleaseAsync(Model.ArtistId, Model.FolderName);
        var sortedByTrackPosition = tracks.OrderBy(r => r.JsonTrack.TrackNumber).ToList();

        return sortedByTrackPosition.Select(r => new Track(r));
    }

    public ReleaseDownloadStatus DownloadStatus() => Model.DownloadStatus.ToGql();

    public int DiscCount()
    {
        var discs = Model.JsonRelease.Discs;
        return (discs is { Count: > 0 }) ? discs.Count : 1;
    }

    public IEnumerable<Disc> Discs()
    {
        var discs = Model.JsonRelease.Discs;
        if (discs is { Count: > 0 })
        {
            return discs
                .OrderBy(d => d.DiscNumber)
                .Select(d => new Disc(this, d.DiscNumber, d.Title));
        }
        // Fallback single-disc view
        return new[] { new Disc(this, 1, null) };
    }

    public async Task<bool> IsFullyMissing(ServerLibraryCache cache)
    {
        var tracks = await cache.GetAllTracksForReleaseAsync(Model.ArtistId, Model.FolderName);
        if (tracks.Count == 0) return true;
        return tracks.All(t => string.IsNullOrWhiteSpace(t.JsonTrack.AudioFilePath));
    }

    /// <summary>
    /// Gets the label information for this release
    /// </summary>
    public IEnumerable<JsonLabelInfo> Labels() => Model.JsonRelease.Labels ?? [];

    /// <summary>
    /// MusicBrainz connections for this release. These are persisted in the underlying JSON.
    /// </summary>
    public string? MusicBrainzReleaseGroupId() => Model.JsonRelease.Connections?.MusicBrainzReleaseGroupId;

    public string? MusicBrainzSelectedReleaseId() => Model.JsonRelease.Connections?.MusicBrainzSelectedReleaseId;

    public string? MusicBrainzReleaseIdOverride() => Model.JsonRelease.Connections?.MusicBrainzReleaseIdOverride;
};

public record Disc(Release Release, int DNumber, string? DTitle)
{
    public int DiscNumber() => DNumber;
    public string? Title() => DTitle;
    public async Task<IEnumerable<Track>> Tracks(ServerLibrary.Cache.ServerLibraryCache cache)
    {
        // Use cached tracks for this release and filter by disc number
        var tracks = await cache.GetAllTracksForReleaseAsync(
            Release.Model.ArtistId,
            Release.Model.FolderName
        );
        return tracks
            .Where(t => (t.DiscNumber > 0 ? t.DiscNumber : 1) == DNumber)
            .OrderBy(t => t.JsonTrack.TrackNumber)
            .Select(t => new Track(t));
    }
}
