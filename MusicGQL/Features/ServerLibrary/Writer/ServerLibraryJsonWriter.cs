using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerLibrary.Json;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Writer;

public class ServerLibraryJsonWriter
{
    private const string LibraryRoot = "./Library";

    private static JsonSerializerOptions GetJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private static string GetArtistDir(string artistId)
        => Path.Combine(LibraryRoot, artistId);

    private static string GetArtistJsonPath(string artistId)
        => Path.Combine(GetArtistDir(artistId), "artist.json");

    private static string GetReleaseDir(string artistId, string releaseFolderName)
        => Path.Combine(GetArtistDir(artistId), releaseFolderName);

    private static string GetReleaseJsonPath(string artistId, string releaseFolderName)
        => Path.Combine(GetReleaseDir(artistId, releaseFolderName), "release.json");

    public async Task WriteArtistAsync(JsonArtist artist)
    {
        if (string.IsNullOrWhiteSpace(artist.Id))
            throw new ArgumentException("Artist.Id must be set on JsonArtist before writing.");

        var dir = GetArtistDir(artist.Id);
        Directory.CreateDirectory(dir);
        var path = GetArtistJsonPath(artist.Id);
        var json = JsonSerializer.Serialize(artist, GetJsonOptions());
        await File.WriteAllTextAsync(path, json);
    }

    public async Task WriteReleaseAsync(string artistId, string releaseFolderName, JsonRelease release)
    {
        if (string.IsNullOrWhiteSpace(artistId))
            throw new ArgumentException("artistId is required");
        if (string.IsNullOrWhiteSpace(releaseFolderName))
            throw new ArgumentException("releaseFolderName is required");

        var dir = GetReleaseDir(artistId, releaseFolderName);
        Directory.CreateDirectory(dir);
        var path = GetReleaseJsonPath(artistId, releaseFolderName);
        var json = JsonSerializer.Serialize(release, GetJsonOptions());
        await File.WriteAllTextAsync(path, json);
    }

    public async Task UpdateReleaseAsync(string artistId, string releaseFolderName, Action<JsonRelease> update)
    {
        var path = GetReleaseJsonPath(artistId, releaseFolderName);
        if (!File.Exists(path)) return;

        var text = await File.ReadAllTextAsync(path);
        var release = JsonSerializer.Deserialize<JsonRelease>(text, GetJsonOptions());
        if (release is null) return;

        update(release);

        var json = JsonSerializer.Serialize(release, GetJsonOptions());
        await File.WriteAllTextAsync(path, json);
    }
}

