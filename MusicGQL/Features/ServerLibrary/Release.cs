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
        // Prefer cached discs if present
        if (Model.Discs is { Count: > 0 }) return Model.Discs.Count;
        var discs = Model.JsonRelease.Discs;
        return (discs is { Count: > 0 }) ? discs.Count : 1;
    }

    public IEnumerable<Disc> Discs()
    {
        // Prefer cached discs if present; fallback to single synthetic disc using cached tracks
        var discs = Model.Discs;
        if (discs is { Count: > 0 })
        {
            return discs
                .OrderBy(d => d.DiscNumber)
                .Select(d => new Disc(d));
        }

        // If JSON contains discs but cached discs are not built (e.g., direct construction in tests), map minimal discs from JSON
        var jsonDiscs = Model.JsonRelease.Discs;
        if (jsonDiscs is { Count: > 0 })
        {
            return jsonDiscs
                .OrderBy(d => d.DiscNumber)
                .Select(d => new Disc(new CachedDisc { DiscNumber = d.DiscNumber <= 0 ? 1 : d.DiscNumber, Title = d.Title, Tracks = new() }));
        }

        // Fallback: group cached tracks by disc number
        var groups = Model.Tracks
            .GroupBy(t => t.DiscNumber > 0 ? t.DiscNumber : 1)
            .OrderBy(g => g.Key)
            .ToList();
        if (groups.Count == 0)
        {
            return new[] { new Disc(new CachedDisc { DiscNumber = 1, Title = null, Tracks = new() }) };
        }
        return groups.Select(g => new Disc(new CachedDisc { DiscNumber = g.Key, Title = null, Tracks = g.OrderBy(t => t.TrackNumber).ToList() }));
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

public record Disc([property: GraphQLIgnore] CachedDisc Model)
{
    public int DiscNumber() => Model.DiscNumber;
    public string? Title() => Model.Title;
    public IEnumerable<Track> Tracks() => Model.Tracks.OrderBy(t => t.TrackNumber).Select(t => new Track(t));
}
