using MusicGQL.Features.ServerLibrary.Reader;
using MusicGQL.Features.ServerLibrary.Cache;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public class LibraryMaintenanceCoordinator(
    ServerLibraryFileSystemScanner scanner,
    IFolderIdentityService identifier,
    IImportExecutor importer,
    ServerLibraryCache cache
)
{
    public class MaintenanceResult
    {
        public bool Success { get; set; }
        public int ArtistsCreated { get; set; }
        public int ReleasesCreated { get; set; }
        public List<string> Notes { get; set; } = [];
        public string? ErrorMessage { get; set; }
    }

    public async Task<MaintenanceResult> RunAsync()
    {
        var result = new MaintenanceResult { Success = true };
        try
        {
            var plan = await scanner.ScanAsync();
            foreach (var artist in plan.ArtistsNeedingWork)
            {
                var releaseDirs = artist.Releases.Select(r => r.ReleaseDir).ToList();
                var idArtist = await identifier.IdentifyArtistAsync(artist.ArtistDir, releaseDirs);
                if (idArtist == null)
                {
                    result.Notes.Add($"Could not identify artist for '{artist.ArtistDir}'");
                    continue;
                }

                if (artist.MissingArtistJson)
                {
                    await importer.ImportArtistIfMissingAsync(artist.ArtistDir, idArtist.MusicBrainzArtistId, idArtist.ArtistDisplayName);
                    result.ArtistsCreated++;
                    result.Notes.Add($"Created artist.json for '{Path.GetFileName(artist.ArtistDir)}'");
                }

                foreach (var rel in artist.Releases.Where(r => r.MissingReleaseJson))
                {
                    var idRel = await identifier.IdentifyReleaseAsync(idArtist.ArtistDisplayName, idArtist.MusicBrainzArtistId, rel.ReleaseDir);
                    if (idRel == null)
                    {
                        result.Notes.Add($"Could not match release for '{rel.ReleaseDir}'");
                        continue;
                    }

                    await importer.ImportReleaseIfMissingAsync(artist.ArtistDir, rel.ReleaseDir, idRel.ReleaseGroupId, idRel.Title, idRel.PrimaryType);
                    result.ReleasesCreated++;
                    result.Notes.Add($"Created release.json for '{Path.GetFileName(artist.ArtistDir)}/{Path.GetFileName(rel.ReleaseDir)}'");
                }
            }

            if (result.ArtistsCreated > 0 || result.ReleasesCreated > 0)
            {
                await cache.UpdateCacheAsync();
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }
}


