using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Features.ServerSettings;
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
            Model.DiscNumber,
            Model.TrackNumber
        );

    public string? AudioFormat([Service] ServerSettingsAccessor serverSettingsAccessor)
    {
        var (fullPath, ext) = TryGetAudioFilePath(serverSettingsAccessor);
        return ext;
    }

    public int? AudioBitrateKbps([Service] ServerSettingsAccessor serverSettingsAccessor)
    {
        var (fullPath, ext) = TryGetAudioFilePath(serverSettingsAccessor);
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
                return Audio.Mp3HeaderReader.TryReadBitrateKbps(fs);
            }
        }
        catch
        {
            /* ignore and return null */
        }

        return null;
    }

    public bool IsLosslessFormat([Service] ServerSettingsAccessor serverSettingsAccessor)
    {
        var (_, ext) = TryGetAudioFilePath(serverSettingsAccessor);
        if (ext is null)
            return false;
        return ext is "flac" or "wav" or "alac" or "aiff";
    }

    public string AudioQualityLabel([Service] ServerSettingsAccessor serverSettingsAccessor)
    {
        var format = AudioFormat(serverSettingsAccessor)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(format))
            return string.Empty;
        if (IsLosslessFormat(serverSettingsAccessor))
            return "lossless";
        if (format == "mp3")
        {
            var kbps = AudioBitrateKbps(serverSettingsAccessor);
            if (kbps is int k && k > 0)
                return $"mp3 {k}kbps";
        }

        return format!;
    }

    private (string? fullPath, string? ext) TryGetAudioFilePath(
        ServerSettingsAccessor serverSettingsAccessor
    )
    {
        var rel = Model.JsonTrack.AudioFilePath;
        if (string.IsNullOrWhiteSpace(rel))
            return (null, null);
        if (rel.StartsWith("./"))
            rel = rel[2..];
        string libraryPath = string.Empty;
        try
        {
            libraryPath = serverSettingsAccessor.GetAsync().GetAwaiter().GetResult().LibraryPath;
        }
        catch { }
        var baseDir = Path.Combine(libraryPath, Model.ArtistId, Model.ReleaseFolderName);
        var full = Path.Combine(baseDir, rel);
        if (!File.Exists(full))
            return (null, null);
        var ext = Path.GetExtension(full).TrimStart('.').ToLowerInvariant();
        return (full, ext);
    }
}
