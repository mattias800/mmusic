using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Reader;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public class LibraryMaintenanceCoordinator(
    ServerLibraryFileSystemScanner scanner,
    IFolderIdentityService identifier,
    IImportExecutor importer,
    ServerLibraryCache cache,
    LastFmEnrichmentService enrichment,
    ServerLibrary.MediaFileAssignmentService assigner
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
            var artistsToEnrich = new Dictionary<string, string>(); // artistDir -> mbArtistId
            foreach (var artist in plan.ArtistsNeedingWork)
            {
                var releaseDirs = artist.Releases.Select(r => r.ReleaseDir).ToList();
                var idArtist = await identifier.IdentifyArtistAsync(artist.ArtistDir, releaseDirs);
                if (idArtist == null)
                {
                    result.Notes.Add($"Could not identify artist for '{artist.ArtistDir}'");
                    continue;
                }

                var importedAnyReleaseForThisArtist = false;
                if (artist.MissingArtistJson)
                {
                    await importer.ImportOrEnrichArtistAsync(
                        artist.ArtistDir,
                        idArtist.MusicBrainzArtistId,
                        idArtist.ArtistDisplayName
                    );
                    result.ArtistsCreated++;
                    result.Notes.Add(
                        $"Created artist.json for '{Path.GetFileName(artist.ArtistDir)}'"
                    );
                    artistsToEnrich[artist.ArtistDir] = idArtist.MusicBrainzArtistId;

                    // Import all eligible release groups (Albums, EPs, Singles) even if no audio exists on disk
                    try
                    {
                        var createdCount = await importer.ImportEligibleReleaseGroupsAsync(
                            artist.ArtistDir,
                            idArtist.MusicBrainzArtistId
                        );
                        result.ReleasesCreated += createdCount;
                        if (createdCount > 0)
                        {
                            result.Notes.Add(
                                $"Imported {createdCount} eligible release group(s) for '{Path.GetFileName(artist.ArtistDir)}'"
                            );
                        }
                        importedAnyReleaseForThisArtist =
                            importedAnyReleaseForThisArtist || createdCount > 0;
                    }
                    catch (Exception ex)
                    {
                        result.Notes.Add($"Failed to import eligible release groups: {ex.Message}");
                    }
                }

                foreach (var rel in artist.Releases.Where(r => r.MissingReleaseJson))
                {
                    var idRel = await identifier.IdentifyReleaseAsync(
                        idArtist.ArtistDisplayName,
                        idArtist.MusicBrainzArtistId,
                        rel.ReleaseDir
                    );
                    if (idRel == null)
                    {
                        result.Notes.Add($"Could not match release for '{rel.ReleaseDir}'");
                        continue;
                    }

                    await importer.ImportReleaseIfMissingAsync(
                        artist.ArtistDir,
                        rel.ReleaseDir,
                        idRel.ReleaseGroupId,
                        idRel.Title,
                        idRel.PrimaryType
                    );
                    result.ReleasesCreated++;
                    result.Notes.Add(
                        $"Created release.json for '{Path.GetFileName(artist.ArtistDir)}/{Path.GetFileName(rel.ReleaseDir)}'"
                    );

                    // Assign media files to tracks (always overwrite existing references)
                    var artistId = Path.GetFileName(artist.ArtistDir) ?? string.Empty;
                    var releaseFolderName = Path.GetFileName(rel.ReleaseDir) ?? string.Empty;
                    try
                    {
                        await assigner.AssignAsync(artistId, releaseFolderName);
                    }
                    catch (Exception ex)
                    {
                        result.Notes.Add(
                            $"Failed assigning media files for '{artistId}/{releaseFolderName}': {ex.Message}"
                        );
                    }

                    importedAnyReleaseForThisArtist = true;
                }

                if (importedAnyReleaseForThisArtist)
                {
                    artistsToEnrich[artist.ArtistDir] = idArtist.MusicBrainzArtistId;
                }

                // Update existing release.json files to include ArtistName field
                try
                {
                    var artistJsonPath = Path.Combine(artist.ArtistDir, "artist.json");
                    if (File.Exists(artistJsonPath))
                    {
                        var artistText = await File.ReadAllTextAsync(artistJsonPath);
                        var jsonArtist = JsonSerializer.Deserialize<JsonArtist>(
                            artistText,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                PropertyNameCaseInsensitive = true,
                                Converters =
                                {
                                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                                },
                            }
                        );

                        if (jsonArtist?.Name != null)
                        {
                            var updatedReleases = 0;
                            foreach (var rel in artist.Releases.Where(r => !r.MissingReleaseJson))
                            {
                                var releaseJsonPath = Path.Combine(rel.ReleaseDir, "release.json");
                                if (File.Exists(releaseJsonPath))
                                {
                                    try
                                    {
                                        var releaseText = await File.ReadAllTextAsync(
                                            releaseJsonPath
                                        );
                                        var releaseJson = JsonSerializer.Deserialize<JsonRelease>(
                                            releaseText,
                                            new JsonSerializerOptions
                                            {
                                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                PropertyNameCaseInsensitive = true,
                                                Converters =
                                                {
                                                    new JsonStringEnumConverter(
                                                        JsonNamingPolicy.CamelCase
                                                    ),
                                                },
                                            }
                                        );

                                        if (
                                            releaseJson != null
                                            && string.IsNullOrWhiteSpace(releaseJson.ArtistName)
                                        )
                                        {
                                            releaseJson.ArtistName = jsonArtist.Name;
                                            var updatedText = JsonSerializer.Serialize(
                                                releaseJson,
                                                new JsonSerializerOptions
                                                {
                                                    PropertyNamingPolicy =
                                                        JsonNamingPolicy.CamelCase,
                                                    WriteIndented = true,
                                                    Converters =
                                                    {
                                                        new JsonStringEnumConverter(
                                                            JsonNamingPolicy.CamelCase
                                                        ),
                                                    },
                                                }
                                            );
                                            await File.WriteAllTextAsync(
                                                releaseJsonPath,
                                                updatedText
                                            );
                                            updatedReleases++;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        result.Notes.Add(
                                            $"Failed to update release.json for '{Path.GetFileName(rel.ReleaseDir)}': {ex.Message}"
                                        );
                                    }
                                }
                            }

                            if (updatedReleases > 0)
                            {
                                result.Notes.Add(
                                    $"Updated ArtistName field in {updatedReleases} existing release.json files for '{Path.GetFileName(artist.ArtistDir)}'"
                                );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Notes.Add(
                        $"Failed to update ArtistName fields for '{Path.GetFileName(artist.ArtistDir)}': {ex.Message}"
                    );
                }
            }

            if (result.ArtistsCreated > 0 || result.ReleasesCreated > 0)
            {
                // Enrich affected artists so topTracks link to newly imported releases
                foreach (var kvp in artistsToEnrich)
                {
                    try
                    {
                        await enrichment.EnrichArtistAsync(kvp.Key, kvp.Value);
                    }
                    catch (Exception ex)
                    {
                        result.Notes.Add(
                            $"Enrichment failed for '{Path.GetFileName(kvp.Key)}': {ex.Message}"
                        );
                    }
                }

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
