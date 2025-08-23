using System.Text;
using System.Collections.Concurrent;
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
    // Global, per-file locks to serialize writes across all logger instances
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new(StringComparer.OrdinalIgnoreCase);

    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _fileLock;
    private readonly string _path;

    public DownloadLogger(string logFilePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        _path = Path.GetFullPath(logFilePath);
        _fileLock = FileLocks.GetOrAdd(_path, _ => new SemaphoreSlim(1, 1));
        // Allow shared write access; we rely on the per-file semaphore to serialize lines
        _writer = new StreamWriter(new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
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
        _fileLock.Wait();
        try
        {
            _writer.WriteLine(line);
            // Ensure the underlying stream flushes promptly in multi-writer scenarios
            _writer.Flush();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public void Dispose()
    {
        try { _writer?.Dispose(); } catch { }
        // Do not remove the semaphore from the dictionary to avoid races while other instances may be using it
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

    public async Task<string?> GetServiceLogFilePathAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var settings = await serverSettingsAccessor.GetAsync();
        var root = settings.LogsFolderPath;
        if (string.IsNullOrWhiteSpace(root)) return null;
        Directory.CreateDirectory(root);
        var safe = SanitizeForFileName(serviceName);
        return Path.Combine(root, $"{safe}.log");
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

