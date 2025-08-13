using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Audio;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Share;

public class ArtistShareManifestService(ServerLibraryCache cache)
{
    private record Manifest(
        int SchemaVersion,
        ManifestArtist Artist,
        string Updated,
        ManifestCoverage Coverage,
        ManifestAudio Audio,
        ManifestTotals Totals,
        List<ManifestRelease> Releases,
        string? Notes
    );

    private record ManifestArtist(string Name, string Slug, string ArtistId, string? MusicBrainzId);
    private record ManifestCoverage(string Scope, List<string> Include, List<string> Exclude);
    private record ManifestAudio(string Format, string Bitrate, List<string> Codecs);
    private record ManifestTotals(int Releases, int Tracks, long SizeBytes);
    private record ManifestRelease(
        string Title,
        int? Year,
        string Type,
        string Path,
        string Format,
        string Bitrate,
        int Tracks
    );

    public async Task<(string tagFileName, string manifestPath)> GenerateForArtistAsync(string artistId, CancellationToken cancellationToken = default)
    {
        var artist = await cache.GetArtistByIdAsync(artistId);
        if (artist is null)
            throw new InvalidOperationException("Artist not found: " + artistId);

        var artistDir = artist.ArtistPath;
        Directory.CreateDirectory(artistDir);

        var updatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var includedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codecs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var exclude = new List<string> { "demos", "bootlegs", "unofficial" };

        int totalTracks = 0;
        long totalSizeBytes = 0;

        bool allLossless = true;
        bool allMp3320 = true;
        bool anyMp3 = false;
        bool anyLossy = false;
        bool anyFiles = false;

        var releaseManifests = new List<ManifestRelease>();

        foreach (var release in artist.Releases)
        {
            var typeStr = release.Type switch
            {
                Json.JsonReleaseType.Album => "album",
                Json.JsonReleaseType.Ep => "ep",
                Json.JsonReleaseType.Single => "single",
                _ => "album",
            };
            includedTypes.Add(typeStr);

            int releaseTrackCount = 0;
            string releaseFormat = "mixed";
            string releaseBitrate = "mixed";

            bool releaseAllLossless = true;
            bool releaseAllMp3320 = true;
            bool releaseAnyMp3 = false;
            var releaseCodecs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var track in release.Tracks)
            {
                var relPath = track.JsonTrack.AudioFilePath;
                if (string.IsNullOrWhiteSpace(relPath))
                    continue;
                if (relPath.StartsWith("./"))
                    relPath = relPath[2..];
                var full = Path.Combine("./Library", release.ArtistId, release.FolderName, relPath);
                var ext = Path.GetExtension(full).TrimStart('.').ToLowerInvariant();
                if (!File.Exists(full))
                    continue;

                anyFiles = true;
                releaseTrackCount++;
                totalTracks++;
                try
                {
                    totalSizeBytes += new FileInfo(full).Length;
                }
                catch { }

                releaseCodecs.Add(ext);
                codecs.Add(ext);

                var isLossless = ext is "flac" or "wav" or "alac" or "aiff";
                if (!isLossless)
                {
                    releaseAllLossless = false;
                    allLossless = false;
                    anyLossy = true;
                }

                if (string.Equals(ext, "mp3", StringComparison.OrdinalIgnoreCase))
                {
                    releaseAnyMp3 = true;
                    anyMp3 = true;
                    int? kbps = null;
                    try
                    {
                        using var fs = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read);
                        kbps = Mp3HeaderReader.TryReadBitrateKbps(fs);
                    }
                    catch { }

                    if (kbps != 320)
                    {
                        releaseAllMp3320 = false;
                        allMp3320 = false;
                    }
                }
            }

            if (releaseTrackCount > 0)
            {
                if (releaseAllLossless && releaseCodecs.SetEquals(["flac"]))
                {
                    releaseFormat = "flac";
                    releaseBitrate = "lossless";
                }
                else if (releaseAllLossless && releaseCodecs.IsSubsetOf(["flac", "wav", "alac", "aiff"]))
                {
                    releaseFormat = "mixed";
                    releaseBitrate = "lossless";
                }
                else if (releaseAnyMp3 && releaseAllMp3320 && releaseCodecs.SetEquals(["mp3"]))
                {
                    releaseFormat = "mp3";
                    releaseBitrate = "320";
                }
                else if (releaseAnyMp3 && releaseCodecs.Contains("mp3") && !releaseAllMp3320)
                {
                    releaseFormat = releaseCodecs.Count == 1 ? "mp3" : "mixed";
                    releaseBitrate = "mixed";
                }
                else
                {
                    releaseFormat = releaseCodecs.Count == 1 ? releaseCodecs.First() : "mixed";
                    releaseBitrate = releaseAllLossless ? "lossless" : "mixed";
                }

                int? year = null;
                if (int.TryParse(release.JsonRelease.FirstReleaseYear, out var parsedYear))
                    year = parsedYear;

                releaseManifests.Add(new ManifestRelease(
                    Title: release.Title,
                    Year: year,
                    Type: typeStr,
                    Path: Path.Combine(artist.Name, release.FolderName),
                    Format: releaseFormat,
                    Bitrate: releaseBitrate,
                    Tracks: releaseTrackCount
                ));
            }
        }

        var scope = string.Join('+', includedTypes.OrderBy(x => x));
        if (string.IsNullOrWhiteSpace(scope))
            scope = "albums";

        string overallFormat;
        string overallBitrate;

        if (!anyFiles)
        {
            overallFormat = "mixed";
            overallBitrate = "mixed";
        }
        else if (allLossless && codecs.SetEquals(["flac"]))
        {
            overallFormat = "flac";
            overallBitrate = "lossless";
        }
        else if (allLossless && codecs.IsSubsetOf(["flac", "wav", "alac", "aiff"]))
        {
            overallFormat = "mixed";
            overallBitrate = "lossless";
        }
        else if (anyMp3 && allMp3320 && codecs.SetEquals(["mp3"]))
        {
            overallFormat = "mp3";
            overallBitrate = "320";
        }
        else
        {
            overallFormat = codecs.Count == 1 ? codecs.First() : "mixed";
            overallBitrate = allLossless ? "lossless" : "mixed";
        }

        var manifest = new Manifest(
            SchemaVersion: 1,
            Artist: new ManifestArtist(
                Name: artist.Name,
                Slug: Slugify(artist.Name),
                ArtistId: artist.Id,
                MusicBrainzId: artist.JsonArtist.Connections?.MusicBrainzArtistId
            ),
            Updated: updatedDate,
            Coverage: new ManifestCoverage(
                Scope: scope,
                Include: includedTypes.OrderBy(x => x).ToList(),
                Exclude: exclude
            ),
            Audio: new ManifestAudio(
                Format: overallFormat,
                Bitrate: overallBitrate,
                Codecs: codecs.OrderBy(x => x).ToList()
            ),
            Totals: new ManifestTotals(
                Releases: releaseManifests.Count,
                Tracks: totalTracks,
                SizeBytes: totalSizeBytes
            ),
            Releases: releaseManifests.OrderBy(r => r.Year ?? int.MaxValue).ThenBy(r => r.Title).ToList(),
            Notes: $"Snapshot of library as of {updatedDate}"
        );

        // Write manifest JSON
        var manifestPath = Path.Combine(artistDir, "mmusic.manifest.json");
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        });
        await File.WriteAllTextAsync(manifestPath, json, cancellationToken);

        // Remove old mmusic tag files for this artist
        try
        {
            foreach (var existing in Directory.GetFiles(artistDir, "mmusic - * - [v1].nfo"))
            {
                try { File.Delete(existing); } catch { }
            }
        }
        catch { }

        // Write tag file (.nfo) with searchable filename
        var safeArtistName = SanitizeForFileName(artist.Name);
        var tagFileName = $"mmusic - {safeArtistName} - [scope={scope}][format={overallFormat}][bitrate={overallBitrate}][updated={updatedDate}][v1].nfo";
        var tagFilePath = Path.Combine(artistDir, tagFileName);
        var tagContents = $"mmusic share tag for {artist.Name}\nupdated: {updatedDate}\nscope: {scope}\nformat: {overallFormat}\nbitrate: {overallBitrate}\n";
        await File.WriteAllTextAsync(tagFilePath, tagContents, cancellationToken);

        return (tagFileName, manifestPath);
    }

    private static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var invalid = Path.GetInvalidFileNameChars();
        var filtered = new string(input.Where(c => !invalid.Contains(c)).ToArray());
        filtered = filtered.Replace(' ', '-');
        while (filtered.Contains("--")) filtered = filtered.Replace("--", "-");
        return filtered.ToLowerInvariant();
    }

    private static string SanitizeForFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var invalid = Path.GetInvalidFileNameChars();
        var filtered = new string(input.Where(c => !invalid.Contains(c)).ToArray());
        return filtered.Trim();
    }
}


