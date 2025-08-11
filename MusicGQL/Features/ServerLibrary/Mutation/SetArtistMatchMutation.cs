using System.Text.Json;
using System.Text.Json.Serialization;
using HotChocolate.Subscriptions;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class SetArtistMatchMutation
{
    public async Task<SetArtistMusicBrainzMatchResult> SetArtistMusicBrainzMatch(
        [Service] ServerLibraryCache cache,
        [Service] IImportExecutor importExecutor,
        [Service] LastFmEnrichmentService enrichmentService,
        [Service] ITopicEventSender eventSender,
        SetArtistMusicBrainzMatchInput input
    )
    {
        var artist = await cache.GetArtistByIdAsync(input.ArtistId);
        if (artist == null)
            return new SetArtistMusicBrainzMatchError("Artist not found");

        var artistDir = artist.ArtistPath;
        var artistJsonPath = Path.Combine(artistDir, "artist.json");
        JsonArtist? json = null;
        try
        {
            if (File.Exists(artistJsonPath))
            {
                var text = await File.ReadAllTextAsync(artistJsonPath);
                json = JsonSerializer.Deserialize<JsonArtist>(text, GetJsonOptions());
            }
        }
        catch { }

        json ??= new JsonArtist { Id = artist.Id, Name = artist.Name };
        json.Connections ??= new JsonArtistServiceConnections();
        json.Connections.MusicBrainzArtistId = input.MusicBrainzArtistId;

        try
        {
            var updated = JsonSerializer.Serialize(json, GetJsonOptions());
            await File.WriteAllTextAsync(artistJsonPath, updated);
        }
        catch (Exception ex)
        {
            return new SetArtistMusicBrainzMatchError($"Failed to write artist.json: {ex.Message}");
        }

        // Enrich to refresh top tracks/photos and connections based on new MBID
        try
        {
            await importExecutor.ImportOrEnrichArtistAsync(artistDir, input.MusicBrainzArtistId, json.Name);
            await enrichmentService.EnrichArtistAsync(artistDir, input.MusicBrainzArtistId);
        }
        catch { }

        // Since MBID is our source of truth for releases, clear all existing release folders and re-import
        try
        {
            var subDirs = Directory.GetDirectories(artistDir);
            foreach (var dir in subDirs)
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
            // Re-import all eligible release groups for this artist
            try { await importExecutor.ImportEligibleReleaseGroupsAsync(artistDir, input.MusicBrainzArtistId); } catch { }
        }
        catch { }

        await cache.UpdateCacheAsync();

        var updatedArtist = await cache.GetArtistByIdAsync(input.ArtistId);
        if (updatedArtist == null)
            return new SetArtistMusicBrainzMatchError("Artist not found after update");

        return new SetArtistMusicBrainzMatchSuccess(new Artists.Artist(updatedArtist));
    }

    public async Task<SetArtistSpotifyMatchResult> SetArtistSpotifyMatch(
        [Service] ServerLibraryCache cache,
        [Service] ITopicEventSender eventSender,
        [Service] LastFmEnrichmentService enrichmentService,
        SetArtistSpotifyMatchInput input
    )
    {
        var artist = await cache.GetArtistByIdAsync(input.ArtistId);
        if (artist == null)
            return new SetArtistSpotifyMatchError("Artist not found");

        var artistDir = artist.ArtistPath;
        var artistJsonPath = Path.Combine(artistDir, "artist.json");
        JsonArtist? json = null;
        try
        {
            if (File.Exists(artistJsonPath))
            {
                var text = await File.ReadAllTextAsync(artistJsonPath);
                json = JsonSerializer.Deserialize<JsonArtist>(text, GetJsonOptions());
            }
        }
        catch { }

        json ??= new JsonArtist { Id = artist.Id, Name = artist.Name };
        json.Connections ??= new JsonArtistServiceConnections();
        json.Connections.SpotifyId = input.SpotifyArtistId;

        try
        {
            var updated = JsonSerializer.Serialize(json, GetJsonOptions());
            await File.WriteAllTextAsync(artistJsonPath, updated);
        }
        catch (Exception ex)
        {
            return new SetArtistSpotifyMatchError($"Failed to write artist.json: {ex.Message}");
        }

        // If MBID exists, run enrichment to update thumbs/top tracks with Spotify fallback
        var mbid = json.Connections.MusicBrainzArtistId;
        if (!string.IsNullOrWhiteSpace(mbid))
        {
            try { await enrichmentService.EnrichArtistAsync(artistDir, mbid!); } catch { }
        }

        await cache.UpdateCacheAsync();
        var updatedArtist = await cache.GetArtistByIdAsync(input.ArtistId);
        if (updatedArtist == null)
            return new SetArtistSpotifyMatchError("Artist not found after update");

        return new SetArtistSpotifyMatchSuccess(new Artists.Artist(updatedArtist));
    }

    private static JsonSerializerOptions GetJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}

public record SetArtistMusicBrainzMatchInput(string ArtistId, string MusicBrainzArtistId);
public record SetArtistSpotifyMatchInput(string ArtistId, string SpotifyArtistId);

[UnionType("SetArtistMusicBrainzMatchResult")]
public abstract record SetArtistMusicBrainzMatchResult;
public record SetArtistMusicBrainzMatchSuccess(Artists.Artist Artist) : SetArtistMusicBrainzMatchResult;
public record SetArtistMusicBrainzMatchError(string Message) : SetArtistMusicBrainzMatchResult;

[UnionType("SetArtistSpotifyMatchResult")]
public abstract record SetArtistSpotifyMatchResult;
public record SetArtistSpotifyMatchSuccess(Artists.Artist Artist) : SetArtistSpotifyMatchResult;
public record SetArtistSpotifyMatchError(string Message) : SetArtistSpotifyMatchResult;


