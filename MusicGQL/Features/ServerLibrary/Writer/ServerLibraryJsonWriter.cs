using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerSettings;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Writer;

public class ServerLibraryJsonWriter(ServerSettingsAccessor serverSettingsAccessor)
{
    private async Task<string> GetLibraryRootAsync()
    {
        try
        {
            var s = await serverSettingsAccessor.GetAsync();
            return s.LibraryPath;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static JsonSerializerOptions GetJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private async Task<string> GetArtistDirAsync(string artistId)
    {
        var root = await GetLibraryRootAsync();
        return Path.Combine(root, artistId);
    }

    private async Task<string> GetArtistJsonPathAsync(string artistId)
        => Path.Combine(await GetArtistDirAsync(artistId), "artist.json");

    private async Task<string> GetReleaseDirAsync(string artistId, string releaseFolderName)
        => Path.Combine(await GetArtistDirAsync(artistId), releaseFolderName);

    private async Task<string> GetReleaseJsonPathAsync(string artistId, string releaseFolderName)
        => Path.Combine(await GetReleaseDirAsync(artistId, releaseFolderName), "release.json");

    public async Task WriteArtistAsync(JsonArtist artist)
    {
        if (string.IsNullOrWhiteSpace(artist.Id))
            throw new ArgumentException("Artist.Id must be set on JsonArtist before writing.");

        var dir = await GetArtistDirAsync(artist.Id);
        Directory.CreateDirectory(dir);
        var path = await GetArtistJsonPathAsync(artist.Id);
        var json = JsonSerializer.Serialize(artist, GetJsonOptions());
        await File.WriteAllTextAsync(path, json);
    }

    public async Task UpdateArtistAsync(string artistId, Action<JsonArtist> update)
    {
        var path = await GetArtistJsonPathAsync(artistId);
        if (!File.Exists(path)) return;

        var text = await File.ReadAllTextAsync(path);
        var artist = JsonSerializer.Deserialize<JsonArtist>(text, GetJsonOptions());
        if (artist is null) return;

        update(artist);

        var json = JsonSerializer.Serialize(artist, GetJsonOptions());
        await File.WriteAllTextAsync(path, json);
    }

    public async Task WriteReleaseAsync(string artistId, string releaseFolderName, JsonRelease release)
    {
        if (string.IsNullOrWhiteSpace(artistId))
            throw new ArgumentException("artistId is required");
        if (string.IsNullOrWhiteSpace(releaseFolderName))
            throw new ArgumentException("releaseFolderName is required");

        var dir = await GetReleaseDirAsync(artistId, releaseFolderName);
        Directory.CreateDirectory(dir);
        var path = await GetReleaseJsonPathAsync(artistId, releaseFolderName);
        var json = JsonSerializer.Serialize(release, GetJsonOptions());
        await File.WriteAllTextAsync(path, json);
    }

    public async Task UpdateReleaseAsync(string artistId, string releaseFolderName, Action<JsonRelease> update)
    {
        var path = await GetReleaseJsonPathAsync(artistId, releaseFolderName);
        if (!File.Exists(path)) return;

        var text = await File.ReadAllTextAsync(path);
        var release = JsonSerializer.Deserialize<JsonRelease>(text, GetJsonOptions());
        if (release is null) return;

        update(release);

        var json = JsonSerializer.Serialize(release, GetJsonOptions());
        await File.WriteAllTextAsync(path, json);
    }
}

