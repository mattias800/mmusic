using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using MusicGQL.Features.Artists;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Import.Mutations;

[ExtendObjectType(typeof(Types.Mutation))]
public class ImportSimilarArtistsMutation
{
    public async Task<ImportSimilarArtistsResult> ImportSimilarArtists(
        ImportSimilarArtistsInput input,
        [Service] ServerLibraryCache cache,
        [Service] LibraryImportService importService,
        [Service] ILogger<ImportSimilarArtistsMutation> logger
    )
    {
        try
        {
            var parent = await cache.GetArtistByIdAsync(input.ArtistId);
            if (parent is null)
            {
                return new ImportSimilarArtistsError($"Artist '{input.ArtistId}' not found");
            }

            var similar =
                parent
                    .JsonArtist.SimilarArtists?.Select(sa => sa.MusicBrainzArtistId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Cast<string>()
                    .ToList() ?? new List<string>();

            int importedCount = 0;
            foreach (var mbid in similar)
            {
                // Skip if already in library
                var existing = await cache.GetArtistByMusicBrainzIdAsync(mbid!);
                if (existing != null)
                    continue;

                try
                {
                    var result = await importService.ImportArtistByMusicBrainzIdAsync(mbid);
                    if (result.Success)
                        importedCount++;
                }
                catch (Exception exImport)
                {
                    logger.LogWarning(
                        exImport,
                        "[ImportSimilarArtists] Failed to import similar artist MBID={Mbid}",
                        mbid
                    );
                }
            }

            // Re-read parent to return updated entity
            var updated = await cache.GetArtistByIdAsync(input.ArtistId);
            if (updated is null)
            {
                return new ImportSimilarArtistsError("Parent artist not found after import");
            }

            return new ImportSimilarArtistsSuccess(new Artist(updated), importedCount);
        }
        catch (Exception ex)
        {
            return new ImportSimilarArtistsError($"Unexpected error: {ex.Message}");
        }
    }
}

public record ImportSimilarArtistsInput(string ArtistId);

[UnionType]
public abstract record ImportSimilarArtistsResult;

public record ImportSimilarArtistsSuccess(Artist Artist, int ImportedCount)
    : ImportSimilarArtistsResult;

public record ImportSimilarArtistsError(string Message) : ImportSimilarArtistsResult;
