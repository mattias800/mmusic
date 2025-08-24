using System.Text.Json;
using System.Text.Json.Serialization;

namespace MusicGQL.Features.ServerSettings;

public class LibraryManifestService
{
    public const string ManifestFileName = "mmusic.library.json";

    public async Task<bool> HasManifestAsync(string libraryPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(libraryPath))
                return false;
            var path = System.IO.Path.Combine(libraryPath, ManifestFileName);
            return File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    public async Task CreateManifestAsync(string libraryPath)
    {
        if (string.IsNullOrWhiteSpace(libraryPath))
            throw new InvalidOperationException("Library path is not set");

        Directory.CreateDirectory(libraryPath);
        var path = System.IO.Path.Combine(libraryPath, ManifestFileName);
        if (File.Exists(path))
            return;

        var json = JsonSerializer.Serialize(
            new Manifest
            {
                SchemaVersion = 1,
                CreatedAtUtc = DateTime.UtcNow,
                Host = Environment.MachineName,
            },
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            }
        );
        await File.WriteAllTextAsync(path, json);
    }

    public async Task EnsureWritesAllowedAsync(string libraryPath)
    {
        if (!await HasManifestAsync(libraryPath))
            throw new InvalidOperationException(
                "Library manifest not found. Writes are disabled for safety."
            );
    }

    public class Manifest
    {
        public int SchemaVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? Host { get; set; }
    }
}
