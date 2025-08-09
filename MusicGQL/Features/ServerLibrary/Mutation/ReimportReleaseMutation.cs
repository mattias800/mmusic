using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class ReimportReleaseMutation
{
    public async Task<ReimportReleaseResult> ReimportRelease(
        [Service] ServerLibraryCache cache,
        [Service] MusicGQL.Features.Import.Services.MusicBrainzImportService mbImport,
        [Service] LibraryReleaseImportService releaseImporter,
        string ArtistId,
        string ReleaseFolderName
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(ArtistId, ReleaseFolderName);
        if (release == null)
        {
            return new ReimportReleaseError("Release not found");
        }

        // Delete metadata: release.json and cover art file referenced by it
        var releaseJsonPath = Path.Combine(release.ReleasePath, "release.json");
        string? coverArtRel = release.JsonRelease.CoverArt;
        string? coverArtAbs = null;
        if (!string.IsNullOrWhiteSpace(coverArtRel))
        {
            var rel = coverArtRel.StartsWith("./") ? coverArtRel[2..] : coverArtRel;
            coverArtAbs = Path.Combine(release.ReleasePath, rel);
        }

        try
        {
            if (File.Exists(releaseJsonPath)) File.Delete(releaseJsonPath);
            if (!string.IsNullOrWhiteSpace(coverArtAbs) && File.Exists(coverArtAbs)) File.Delete(coverArtAbs);
        }
        catch (Exception ex)
        {
            return new ReimportReleaseError($"Failed deleting metadata: {ex.Message}");
        }

        // Find release group to import using MusicBrainz (by title under artist MBID)
        var artist = await cache.GetArtistByIdAsync(ArtistId);
        var mbArtistId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
        if (string.IsNullOrWhiteSpace(mbArtistId))
        {
            return new ReimportReleaseError("Artist missing MusicBrainz ID");
        }

        var rgs = await mbImport.GetArtistReleaseGroupsAsync(mbArtistId!);
        var match = rgs.FirstOrDefault(rg => string.Equals(rg.Title, release.Title, StringComparison.OrdinalIgnoreCase));
        if (match == null)
        {
            return new ReimportReleaseError("Could not match release group by title");
        }

        var importResult = await releaseImporter.ImportReleaseGroupAsync(match, Path.GetDirectoryName(release.ReleasePath) ?? Path.Combine("./Library", ArtistId), ArtistId);
        await cache.UpdateCacheAsync();
        return importResult.Success ? new ReimportReleaseSuccess(true) : new ReimportReleaseError(importResult.ErrorMessage ?? "Unknown error");
    }
}

[UnionType("ReimportReleaseResult")]
public abstract record ReimportReleaseResult;

public record ReimportReleaseSuccess(bool Success) : ReimportReleaseResult;

public record ReimportReleaseError(string Message) : ReimportReleaseResult;

