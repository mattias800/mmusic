using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Utils;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary;

public record TrackMedia([property: GraphQLIgnore] CachedTrack Model)
{
    [ID]
    public string Id() => Model.ArtistId + "/" + Model.ReleaseFolderName + "/" + Model.TrackNumber;

    /// <summary>
    /// Gets the audio URL that the server can serve
    /// </summary>
    public string AudioUrl() =>
        LibraryAssetUrlFactory.CreateTrackAudioUrl(
            Model.ArtistId,
            Model.ReleaseFolderName,
            Model.TrackNumber
        );

    public string? AudioFormat()
    {
        var (fullPath, ext) = TryGetAudioFilePath();
        return ext;
    }

    public int? AudioBitrateKbps()
    {
        var (fullPath, ext) = TryGetAudioFilePath();
        if (fullPath is null || ext is null)
            return null;
        try
        {
            if (ext.Equals("mp3", StringComparison.OrdinalIgnoreCase))
            {
                using var fs = new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );
                return Features.ServerLibrary.Audio.Mp3HeaderReader.TryReadBitrateKbps(fs);
            }
        }
        catch
        {
            /* ignore and return null */
        }

        return null;
    }

    public bool IsLosslessFormat()
    {
        var (_, ext) = TryGetAudioFilePath();
        if (ext is null)
            return false;
        return ext is "flac" or "wav" or "alac" or "aiff";
    }

    public string AudioQualityLabel()
    {
        var format = AudioFormat()?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(format))
            return string.Empty;
        if (IsLosslessFormat())
            return "lossless";
        if (format == "mp3")
        {
            var kbps = AudioBitrateKbps();
            if (kbps is int k && k > 0)
                return $"mp3 {k}kbps";
        }

        return format!;
    }

    private (string? fullPath, string? ext) TryGetAudioFilePath()
    {
        var rel = Model.JsonTrack.AudioFilePath;
        if (string.IsNullOrWhiteSpace(rel))
            return (null, null);
        if (rel.StartsWith("./"))
            rel = rel[2..];
        var baseDir = Path.Combine("./Library", Model.ArtistId, Model.ReleaseFolderName);
        var full = Path.Combine(baseDir, rel);
        if (!File.Exists(full))
            return (null, null);
        var ext = Path.GetExtension(full).TrimStart('.').ToLowerInvariant();
        return (full, ext);
    }
}
