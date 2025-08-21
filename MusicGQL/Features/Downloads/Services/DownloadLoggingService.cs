using System.Text;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerSettings;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace MusicGQL.Features.Downloads.Services;

public interface IDownloadLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}

public class NullDownloadLogger : IDownloadLogger
{
    public void Info(string message) { }
    public void Warn(string message) { }
    public void Error(string message) { }
}

public class DownloadLogger : IDownloadLogger, IDisposable
{
    private readonly StreamWriter _writer;
    private readonly object _lock = new();

    public DownloadLogger(string logFilePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        _writer = new StreamWriter(new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true,
            NewLine = Environment.NewLine
        };
        Info("==== Logger started ====");
    }

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var line = $"{DateTime.UtcNow:O} [{level}] {message}";
        lock (_lock)
        {
            _writer.WriteLine(line);
        }
    }

    public void Dispose()
    {
        try { _writer?.Dispose(); } catch { }
    }
}

public class DownloadLogPathProvider(ServerSettingsAccessor serverSettingsAccessor)
{
    public async Task<string?> GetReleaseLogFilePathAsync(string artistName, string releaseTitle, CancellationToken cancellationToken = default)
    {
        var settings = await serverSettingsAccessor.GetAsync();
        var root = settings.LogsFolderPath;
        if (string.IsNullOrWhiteSpace(root)) return null;

        var downloadsRoot = Path.Combine(root, "Downloads");
        var safeArtist = SanitizeForFileName(artistName);
        var safeRelease = SanitizeForFileName(releaseTitle);
        var artistDir = Path.Combine(downloadsRoot, safeArtist);
        Directory.CreateDirectory(artistDir);
        return Path.Combine(artistDir, $"{safeRelease}.log");
    }

    private static string SanitizeForFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var invalid = Path.GetInvalidFileNameChars();
        var filtered = new string(input.Where(c => !invalid.Contains(c)).ToArray());
        // Collapse spaces
        filtered = string.Join(" ", filtered.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return filtered.Trim();
    }
}


